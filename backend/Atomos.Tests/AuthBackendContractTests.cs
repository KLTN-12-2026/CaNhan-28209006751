using System.Security.Claims;
using System.Text.Json;
using Atomos.API.Controllers;
using Atomos.API.Security;
using Atomos.Application.Auth;
using Atomos.Application.Common;
using Atomos.Domain.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Atomos.Tests;

public sealed class AuthBackendContractTests
{
    [Theory]
    [InlineData(typeof(RegisterResponse))]
    [InlineData(typeof(LoginResponse))]
    [InlineData(typeof(GoogleAuthResponse))]
    public void AuthResponsesExposeNoCredentialOrSessionInternals(Type responseType)
    {
        var propertyNames = responseType.GetProperties().Select(property => property.Name).ToArray();

        Assert.DoesNotContain(propertyNames, name => name.Contains("Token", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propertyNames, name => name.Contains("Session", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propertyNames, name => name.Contains("Expires", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SessionEndpointIsAnonymousGetAndReturnsNoAccountDataForAnonymous()
    {
        var method = typeof(AuthController).GetMethod(nameof(AuthController.Session))!;
        Assert.NotEmpty(method.GetCustomAttributes(typeof(HttpGetAttribute), true));
        Assert.NotEmpty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));

        var anonymous = Controller(new StubSessionService(), new ClaimsPrincipal()).Session();
        Assert.Equal(AuthBootstrapResponse.Anonymous, Value(anonymous));
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Student")]
    public void SessionEndpointHydratesSharedContractOnlyFromCookieIdentity(string role)
    {
        var secret = AuthSessionSecret.Generate();
        var principal = Principal(AtomosAuthenticationDefaults.CookieScheme, role);
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim("atomos:session-handle", secret));
        var response = Value(Controller(
            new StubSessionService(),
            principal).Session());
        var json = JsonSerializer.Serialize(response);

        Assert.True(response.Authenticated);
        Assert.Equal(7, response.User!.Id);
        Assert.Equal("current-user", response.User.Username);
        Assert.Equal("current@example.test", response.User.Email);
        Assert.Equal("Current Name", response.User.FullName);
        Assert.Equal("/avatar.png", response.User.Avatar);
        Assert.Equal(role, response.Role);
        Assert.DoesNotContain(secret, json, StringComparison.Ordinal);
        Assert.DoesNotContain("session", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("accessToken", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CookieLogoutRevokesCurrentHandleDeletesCookieAndIsCsrfProtected()
    {
        var handle = AuthSessionSecret.Generate();
        var sessions = new StubSessionService();
        await using var provider = Services(sessions);
        var context = Context(provider, Principal(AtomosAuthenticationDefaults.CookieScheme, "Student"));
        context.Request.Headers.Cookie = $"{AtomosAuthenticationDefaults.CookieName}={handle}";
        var controller = Controller(sessions, context.User, context);

        var result = await controller.Logout(default);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(handle, sessions.RevokedSecret);
        Assert.Equal(AuthSessionRevokeReasons.Logout, sessions.RevokeReason);
        var setCookie = context.Response.Headers.SetCookie.ToString();
        Assert.Contains($"{AtomosAuthenticationDefaults.CookieName}=", setCookie, StringComparison.Ordinal);
        Assert.Contains("expires=", setCookie, StringComparison.OrdinalIgnoreCase);

        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(
                typeof(AuthController).GetMethod(nameof(AuthController.Logout))!.GetCustomAttributes(true)),
            "logout");
        context.SetEndpoint(endpoint);
        context.Request.Method = HttpMethods.Post;
        Assert.True(CookieAntiforgeryMiddleware.RequiresValidation(context));
    }

    [Fact]
    public async Task AnonymousLogoutIsIdempotent()
    {
        var sessions = new StubSessionService();
        await using var provider = Services(sessions);
        var anonymousContext = Context(provider, new ClaimsPrincipal());
        var anonymous = Controller(sessions, anonymousContext.User, anonymousContext);

        Assert.IsType<NoContentResult>(await anonymous.Logout(default));
        Assert.Null(sessions.RevokedSecret);
        Assert.Contains(
            $"{AtomosAuthenticationDefaults.CookieName}=",
            anonymousContext.Response.Headers.SetCookie.ToString(),
            StringComparison.Ordinal);

    }

    [Fact]
    public async Task CompletedGoogleAuthenticationCreatesGoogleSessionCookieWithCleanResponse()
    {
        var sessions = new StubSessionService();
        var response = new GoogleAuthResponse
        {
            UserId = 7,
            Username = "google-user",
            Email = "google@example.test",
            Role = "Student"
        };
        await using var provider = Services(sessions);
        var context = Context(provider, new ClaimsPrincipal());
        var controller = new AuthController(
            new StubAuthService(response),
            new AuthSessionCookieIssuer(sessions),
            sessions)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };

        var result = await controller.Google(new GoogleAuthRequest { IdToken = "verified-google-id-token" }, default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<GoogleAuthResponse>(ok.Value);
        Assert.False(body.RequiresUsername);
        Assert.Equal((7, AuthSessionMethod.Google), sessions.CreatedFor);
        Assert.Contains(
            $"{AtomosAuthenticationDefaults.CookieName}={sessions.CreatedSecret}",
            context.Response.Headers.SetCookie.ToString(),
            StringComparison.Ordinal);
        var json = JsonSerializer.Serialize(body);
        Assert.DoesNotContain("token", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("session", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GoogleUsernamePromptDoesNotCreateApplicationSession()
    {
        var sessions = new StubSessionService();
        var response = new GoogleAuthResponse
        {
            RequiresUsername = true,
            Email = "new-google@example.test"
        };
        await using var provider = Services(sessions);
        var context = Context(provider, new ClaimsPrincipal());
        var controller = new AuthController(
            new StubAuthService(response),
            new AuthSessionCookieIssuer(sessions),
            sessions)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };

        var result = await controller.Google(new GoogleAuthRequest { IdToken = "verified-google-id-token" }, default);

        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Null(sessions.CreatedFor);
        Assert.False(context.Response.Headers.ContainsKey("Set-Cookie"));
    }

    private static AuthBootstrapResponse Value(ActionResult<AuthBootstrapResponse> result) =>
        Assert.IsType<AuthBootstrapResponse>(Assert.IsType<OkObjectResult>(result.Result).Value);

    private static AuthController Controller(
        IAuthSessionService sessions,
        ClaimsPrincipal principal,
        HttpContext? context = null) =>
        new(null!, null!, sessions)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = context ?? new DefaultHttpContext { User = principal }
            }
        };

    private static ClaimsPrincipal Principal(string authenticationType, string role) =>
        new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "7"),
            new Claim(ClaimTypes.Name, "current-user"),
            new Claim(ClaimTypes.Email, "current@example.test"),
            new Claim(ClaimTypes.Role, role),
            new Claim("full_name", "Current Name"),
            new Claim("avatar", "/avatar.png")
        ], authenticationType));

    private static ServiceProvider Services(IAuthSessionService sessions)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(sessions);
        services.AddScoped<AuthSessionCookieEvents>();
        services.AddAuthentication()
            .AddCookie(
                AtomosAuthenticationDefaults.CookieScheme,
                options => AuthCookieConfiguration.Configure(options, TimeSpan.FromMinutes(60)));
        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext Context(IServiceProvider services, ClaimsPrincipal principal) =>
        new()
        {
            RequestServices = services,
            User = principal
        };

    private sealed class StubSessionService : IAuthSessionService
    {
        public string CreatedSecret { get; } = AuthSessionSecret.Generate();
        public (int UserId, AuthSessionMethod Method)? CreatedFor { get; private set; }
        public string? RevokedSecret { get; private set; }
        public string? RevokeReason { get; private set; }

        public Task<CreatedAuthSession> CreateSessionAsync(
            int userId,
            AuthSessionMethod authMethod,
            CancellationToken cancellationToken = default)
        {
            CreatedFor = (userId, authMethod);
            return Task.FromResult(new CreatedAuthSession(
                1,
                CreatedSecret,
                DateTime.UtcNow.AddMinutes(60)));
        }

        public Task<ValidAuthSession?> GetValidSessionAsync(
            string rawSecret,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task RevokeSessionAsync(
            string rawSecret,
            string? reason = null,
            CancellationToken cancellationToken = default)
        {
            RevokedSecret ??= rawSecret;
            RevokeReason ??= reason;
            return Task.CompletedTask;
        }

        public Task<int> RevokeAllUserSessionsAsync(
            int userId,
            string? reason = null,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class StubAuthService(GoogleAuthResponse googleResponse) : IAuthService
    {
        public Task<Result<GoogleAuthResponse>> GoogleAuthAsync(
            GoogleAuthRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(Result<GoogleAuthResponse>.Ok(googleResponse));

        public Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<Result<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<Result<VerifyResetCodeResponse>> VerifyResetCodeAsync(VerifyResetCodeRequest request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<Result<ResetPasswordResponse>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
