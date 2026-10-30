using System.Security.Claims;
using Atomos.API.Security;
using Atomos.Application.Auth;
using Atomos.Domain.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Atomos.Tests;

public sealed class CookieAuthenticationTests
{
    private static readonly DateTime ExpiresAt = new(2026, 10, 5, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CookieConfigurationUsesHostPrefixAndSecureAbsoluteSessionSettings()
    {
        var options = new CookieAuthenticationOptions();

        AuthCookieConfiguration.Configure(options, TimeSpan.FromMinutes(60));

        Assert.Equal("__Host-Atomos.Auth", options.Cookie.Name);
        Assert.True(options.Cookie.HttpOnly);
        Assert.Equal(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
        Assert.Equal(SameSiteMode.Lax, options.Cookie.SameSite);
        Assert.Equal("/", options.Cookie.Path);
        Assert.Null(options.Cookie.Domain);
        Assert.Null(options.Cookie.MaxAge);
        Assert.Equal(TimeSpan.FromMinutes(60), options.ExpireTimeSpan);
        Assert.False(options.SlidingExpiration);
        Assert.Equal(typeof(AuthSessionCookieEvents), options.EventsType);
        Assert.IsType<OpaqueSessionTicketDataFormat>(options.TicketDataFormat);
    }

    [Fact]
    public void OpaqueTicketFormatRoundTripsOnlyCanonicalHandle()
    {
        var handle = AuthSessionSecret.Generate();
        var format = new OpaqueSessionTicketDataFormat();
        var ticket = IssuanceTicket(handle);

        var cookieValue = format.Protect(ticket);
        var unprotected = format.Unprotect(cookieValue);

        Assert.Equal(handle, cookieValue);
        Assert.Equal(43, cookieValue.Length);
        Assert.Equal(handle, unprotected!.Principal.FindFirstValue("atomos:session-handle"));
        Assert.Null(format.Unprotect("malformed"));
        Assert.Throws<InvalidOperationException>(() => format.Protect(
            IssuanceTicket("not-canonical")));
    }

    [Fact]
    public async Task CookieIssuanceEmitsRawOpaqueSessionCookieAndNoPersistentExpiry()
    {
        var handle = AuthSessionSecret.Generate();
        var sessions = new StubSessionService { CreatedSecret = handle };
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IAuthSessionService>(sessions);
        services.AddScoped<AuthSessionCookieEvents>();
        services.AddAuthentication()
            .AddCookie(
                AtomosAuthenticationDefaults.CookieScheme,
                options => AuthCookieConfiguration.Configure(options, TimeSpan.FromMinutes(60)));
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };

        await new AuthSessionCookieIssuer(sessions).IssueAsync(
            context, 42, AuthSessionMethod.Password, default);

        Assert.Single(context.Response.Headers.SetCookie);
        var setCookie = context.Response.Headers.SetCookie.ToString();
        Assert.StartsWith($"__Host-Atomos.Auth={handle};", setCookie, StringComparison.Ordinal);
        Assert.Contains("path=/", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("expires=", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("max-age=", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("42", setCookie, StringComparison.Ordinal);
        Assert.DoesNotContain("Student", setCookie, StringComparison.Ordinal);
        Assert.DoesNotContain("example.test", setCookie, StringComparison.Ordinal);
        Assert.Equal((42, AuthSessionMethod.Password), sessions.CreatedFor);
    }

    [Fact]
    public async Task CookieHandlerAuthenticatesRawHandleThroughServerSideResolution()
    {
        var handle = AuthSessionSecret.Generate();
        var sessions = new StubSessionService { ValidSession = Session(role: "Admin") };
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IAuthSessionService>(sessions);
        services.AddScoped<AuthSessionCookieEvents>();
        services.AddAuthentication(AtomosAuthenticationDefaults.CookieScheme)
            .AddCookie(
                AtomosAuthenticationDefaults.CookieScheme,
                options => AuthCookieConfiguration.Configure(options, TimeSpan.FromMinutes(60)));
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        context.Request.Headers.Cookie = $"__Host-Atomos.Auth={handle}";

        var result = await context.AuthenticateAsync(AtomosAuthenticationDefaults.CookieScheme);

        Assert.True(result.Succeeded);
        Assert.True(result.Principal!.IsInRole("Admin"));
        Assert.Equal("7", result.Principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal(handle, sessions.LookedUpSecret);
    }

    [Fact]
    public async Task ValidSessionBuildsCompatiblePrincipalFromCurrentDatabaseState()
    {
        var handle = AuthSessionSecret.Generate();
        var sessions = new StubSessionService
        {
            ValidSession = Session(role: "Student", fullName: "Current Name")
        };
        var validation = ValidationContext(handle);

        await Events(sessions).ValidatePrincipal(validation);

        Assert.NotNull(validation.Principal);
        Assert.True(validation.Principal!.Identity!.IsAuthenticated);
        Assert.Equal("7", validation.Principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("current-user", validation.Principal.FindFirstValue(ClaimTypes.Name));
        Assert.Equal("current@example.test", validation.Principal.FindFirstValue(ClaimTypes.Email));
        Assert.Equal("Current Name", validation.Principal.FindFirstValue("full_name"));
        Assert.Equal("/avatars/current.png", validation.Principal.FindFirstValue("avatar"));
        Assert.True(validation.Principal.IsInRole("Student"));
        Assert.Equal(handle, sessions.LookedUpSecret);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("malformed")]
    public async Task MissingOrMalformedHandleIsUnauthenticatedWithoutLookup(string? handle)
    {
        var sessions = new StubSessionService { ValidSession = Session() };
        var validation = ValidationContext(handle);

        await Events(sessions).ValidatePrincipal(validation);

        Assert.Null(validation.Principal);
        Assert.Null(sessions.LookedUpSecret);
    }

    [Fact]
    public async Task UnknownRevokedExpiredLockedOrDeletedSessionResultIsUnauthenticated()
    {
        var sessions = new StubSessionService { ValidSession = null };
        var validation = ValidationContext(AuthSessionSecret.Generate());

        await Events(sessions).ValidatePrincipal(validation);

        Assert.Null(validation.Principal);
    }

    [Fact]
    public async Task ResolutionFailureFailsClosedWithoutLeakingException()
    {
        var sessions = new StubSessionService { LookupException = new InvalidOperationException("database detail") };
        var validation = ValidationContext(AuthSessionSecret.Generate());

        await Events(sessions).ValidatePrincipal(validation);

        Assert.Null(validation.Principal);
    }

    [Fact]
    public async Task CookieChallengesRemain401AndForbidRemains403()
    {
        var events = Events(new StubSessionService());
        var login = RedirectContext();
        var denied = RedirectContext();

        await events.RedirectToLogin(login);
        await events.RedirectToAccessDenied(denied);

        Assert.Equal(StatusCodes.Status401Unauthorized, login.Response.StatusCode);
        Assert.Equal(StatusCodes.Status403Forbidden, denied.Response.StatusCode);
    }

    private static AuthSessionCookieEvents Events(IAuthSessionService sessions) =>
        new(sessions, NullLogger<AuthSessionCookieEvents>.Instance);

    private static AuthenticationTicket IssuanceTicket(string handle) => new(
        new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim("atomos:session-handle", handle)],
            AtomosAuthenticationDefaults.CookieScheme)),
        AtomosAuthenticationDefaults.CookieScheme);

    private static CookieValidatePrincipalContext ValidationContext(string? handle)
    {
        var claims = handle is null ? [] : new[] { new Claim("atomos:session-handle", handle) };
        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(new ClaimsIdentity(claims, AtomosAuthenticationDefaults.CookieScheme)),
            AtomosAuthenticationDefaults.CookieScheme);
        return new CookieValidatePrincipalContext(
            new DefaultHttpContext(),
            new AuthenticationScheme(
                AtomosAuthenticationDefaults.CookieScheme,
                displayName: null,
                typeof(CookieAuthenticationHandler)),
            new CookieAuthenticationOptions(),
            ticket);
    }

    private static RedirectContext<CookieAuthenticationOptions> RedirectContext() => new(
        new DefaultHttpContext(),
        new AuthenticationScheme(
            AtomosAuthenticationDefaults.CookieScheme,
            displayName: null,
            typeof(CookieAuthenticationHandler)),
        new CookieAuthenticationOptions(),
        new AuthenticationProperties(),
        redirectUri: "/login");

    private static ValidAuthSession Session(string role = "Student", string? fullName = null) => new(
        1,
        new User
        {
            IDUser = 7,
            Username = "current-user",
            Email = "current@example.test",
            HoTen = fullName,
            Avatar = "/avatars/current.png",
            Role = new Role { TenRole = role }
        },
        AuthSessionMethod.Password,
        ExpiresAt.AddMinutes(-60),
        ExpiresAt);

    private sealed class StubSessionService : IAuthSessionService
    {
        public string CreatedSecret { get; init; } = AuthSessionSecret.Generate();
        public ValidAuthSession? ValidSession { get; init; }
        public Exception? LookupException { get; init; }
        public (int UserId, AuthSessionMethod Method)? CreatedFor { get; private set; }
        public string? LookedUpSecret { get; private set; }

        public Task<CreatedAuthSession> CreateSessionAsync(
            int userId,
            AuthSessionMethod authMethod,
            CancellationToken cancellationToken = default)
        {
            CreatedFor = (userId, authMethod);
            return Task.FromResult(new CreatedAuthSession(1, CreatedSecret, ExpiresAt));
        }

        public Task<ValidAuthSession?> GetValidSessionAsync(
            string rawSecret,
            CancellationToken cancellationToken = default)
        {
            LookedUpSecret = rawSecret;
            return LookupException is null
                ? Task.FromResult(ValidSession)
                : Task.FromException<ValidAuthSession?>(LookupException);
        }

        public Task RevokeSessionAsync(
            string rawSecret,
            string? reason = null,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<int> RevokeAllUserSessionsAsync(
            int userId,
            string? reason = null,
            CancellationToken cancellationToken = default) => Task.FromResult(0);
    }
}
