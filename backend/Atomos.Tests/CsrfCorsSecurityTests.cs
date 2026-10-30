using System.Security.Claims;
using Atomos.API.Controllers;
using Atomos.API.Security;
using Atomos.Application.Auth;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atomos.Tests;

public sealed class CsrfCorsSecurityTests
{
    [Fact]
    public void AntiforgeryConfigurationUsesSecureHostCookieAndSpaHeader()
    {
        var options = new AntiforgeryOptions();

        AuthAntiforgeryConfiguration.Configure(options);

        Assert.Equal("X-CSRF-TOKEN", options.HeaderName);
        Assert.Equal("__Host-Atomos.Csrf", options.Cookie.Name);
        Assert.True(options.Cookie.HttpOnly);
        Assert.Equal(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
        Assert.Equal(SameSiteMode.Strict, options.Cookie.SameSite);
        Assert.Equal("/", options.Cookie.Path);
        Assert.Null(options.Cookie.Domain);
    }

    [Fact]
    public void TokenEndpointIsAnonymousSafeGetWithNoStoreResponse()
    {
        var method = typeof(CsrfController).GetMethod(nameof(CsrfController.GetToken))!;
        var route = Assert.Single(method.GetCustomAttributes(typeof(HttpGetAttribute), true).Cast<HttpGetAttribute>());
        var cache = Assert.Single(method.GetCustomAttributes(typeof(ResponseCacheAttribute), true).Cast<ResponseCacheAttribute>());

        Assert.Equal("csrf", route.Template);
        Assert.NotEmpty(method.GetCustomAttributes(typeof(AllowAnonymousAttribute), true));
        Assert.True(cache.NoStore);
        Assert.Equal(ResponseCacheLocation.None, cache.Location);
    }

    [Fact]
    public void TokenEndpointReturnsOnlyRequestTokenAndNeverAuthCredential()
    {
        using var provider = Services();
        var antiforgery = provider.GetRequiredService<IAntiforgery>();
        var authHandle = AuthSessionSecret.Generate();
        var context = Context(provider);
        context.Request.Headers.Cookie = $"__Host-Atomos.Auth={authHandle}";
        var controller = new CsrfController(antiforgery)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };

        var response = Assert.IsType<OkObjectResult>(controller.GetToken().Result);
        var payload = Assert.IsType<CsrfTokenResponse>(response.Value);
        var setCookie = context.Response.Headers.SetCookie.ToString();

        Assert.False(string.IsNullOrWhiteSpace(payload.Token));
        Assert.NotEqual(authHandle, payload.Token);
        Assert.NotEqual(3, payload.Token.Split('.').Length);
        Assert.DoesNotContain(authHandle, payload.Token, StringComparison.Ordinal);
        Assert.StartsWith("__Host-Atomos.Csrf=", setCookie, StringComparison.Ordinal);
        Assert.DoesNotContain("__Host-Atomos.Auth", setCookie, StringComparison.Ordinal);
        Assert.DoesNotContain(authHandle, setCookie, StringComparison.Ordinal);
    }

    [Fact]
    public void RepeatedUnauthenticatedTokenIssuanceWorks()
    {
        using var provider = Services();
        var first = IssueToken(provider);
        var secondContext = Context(provider);
        secondContext.Request.Headers.Cookie = first.Cookie;
        var second = provider.GetRequiredService<IAntiforgery>().GetAndStoreTokens(secondContext);

        Assert.False(string.IsNullOrWhiteSpace(first.RequestToken));
        Assert.False(string.IsNullOrWhiteSpace(second.RequestToken));
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("HEAD")]
    [InlineData("OPTIONS")]
    [InlineData("TRACE")]
    public async Task SafeCookieRequestsDoNotRequireToken(string method)
    {
        using var provider = Services();

        var result = await InvokeMiddleware(provider, method, AtomosAuthenticationDefaults.CookieScheme);

        Assert.True(result.NextCalled);
        Assert.Equal(StatusCodes.Status200OK, result.Context.Response.StatusCode);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    [InlineData("DELETE")]
    public async Task UnsafeCookieRequestsWithoutTokenAreRejected(string method)
    {
        using var provider = Services();

        var result = await InvokeMiddleware(provider, method, AtomosAuthenticationDefaults.CookieScheme);

        Assert.False(result.NextCalled);
        Assert.Equal(StatusCodes.Status400BadRequest, result.Context.Response.StatusCode);
        result.Context.Response.Body.Position = 0;
        var body = await new StreamReader(result.Context.Response.Body).ReadToEndAsync();
        Assert.Contains("CSRF validation failed", body, StringComparison.Ordinal);
        Assert.DoesNotContain("token", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidCookieAndHeaderPairAllowsUnsafeRequest()
    {
        using var provider = Services();
        var issued = IssueToken(provider, AtomosAuthenticationDefaults.CookieScheme);

        var result = await InvokeMiddleware(
            provider,
            HttpMethods.Post,
            AtomosAuthenticationDefaults.CookieScheme,
            issued.Cookie,
            issued.RequestToken);

        Assert.True(result.NextCalled);
    }

    [Fact]
    public async Task InvalidOrMismatchedTokenIsRejected()
    {
        using var provider = Services();
        var first = IssueToken(provider);
        var second = IssueToken(provider);

        var invalid = await InvokeMiddleware(
            provider, HttpMethods.Post, AtomosAuthenticationDefaults.CookieScheme, first.Cookie, "invalid");
        var mismatched = await InvokeMiddleware(
            provider, HttpMethods.Post, AtomosAuthenticationDefaults.CookieScheme, first.Cookie, second.RequestToken);

        Assert.Equal(StatusCodes.Status400BadRequest, invalid.Context.Response.StatusCode);
        Assert.Equal(StatusCodes.Status400BadRequest, mismatched.Context.Response.StatusCode);
        Assert.False(invalid.NextCalled);
        Assert.False(mismatched.NextCalled);
    }

    [Fact]
    public async Task UnauthenticatedUnsafeRequestsAreNotForcedThroughCsrf()
    {
        using var provider = Services();

        var anonymous = await InvokeMiddleware(provider, HttpMethods.Post, authenticationType: null);

        Assert.True(anonymous.NextCalled);
    }

    [Fact]
    public async Task UnauthenticatedProtectedRequestPreservesDownstream401()
    {
        using var provider = Services();

        var result = await InvokeMiddleware(
            provider,
            HttpMethods.Post,
            authenticationType: null,
            downstreamStatus: StatusCodes.Status401Unauthorized);

        Assert.True(result.NextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.Context.Response.StatusCode);
    }

    [Fact]
    public async Task ValidCookieCsrfPreservesDownstreamAuthorization403()
    {
        using var provider = Services();
        var issued = IssueToken(provider, AtomosAuthenticationDefaults.CookieScheme);

        var result = await InvokeMiddleware(
            provider,
            HttpMethods.Post,
            AtomosAuthenticationDefaults.CookieScheme,
            issued.Cookie,
            issued.RequestToken,
            downstreamStatus: StatusCodes.Status403Forbidden);

        Assert.True(result.NextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, result.Context.Response.StatusCode);
    }

    [Fact]
    public async Task ExplicitMutatingGetRequiresTokenForCookieAuthentication()
    {
        using var provider = Services();
        var issued = IssueToken(provider, AtomosAuthenticationDefaults.CookieScheme);
        var metadata = new RequireCookieAntiforgeryAttribute();

        var missing = await InvokeMiddleware(
            provider, HttpMethods.Get, AtomosAuthenticationDefaults.CookieScheme, metadata: metadata);
        var valid = await InvokeMiddleware(
            provider,
            HttpMethods.Get,
            AtomosAuthenticationDefaults.CookieScheme,
            issued.Cookie,
            issued.RequestToken,
            metadata);

        Assert.Equal(StatusCodes.Status400BadRequest, missing.Context.Response.StatusCode);
        Assert.True(valid.NextCalled);
    }

    [Fact]
    public void DevelopmentCorsUsesExactVerifiedOriginAndProductionDefaultsClosed()
    {
        var development = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(RepoRoot(), "backend", "Atomos.API", "appsettings.Development.json"))
            .Build();
        var production = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(RepoRoot(), "backend", "Atomos.API", "appsettings.Example.json"))
            .Build();

        Assert.Equal(["https://localhost:5173"], FrontendCorsPolicy.ReadAllowedOrigins(development));
        Assert.Empty(FrontendCorsPolicy.ReadAllowedOrigins(production));
    }

    [Fact]
    public void CorsPolicyAllowsExactCredentialedOriginAndRejectsUntrustedOrigin()
    {
        var builder = new CorsPolicyBuilder();
        FrontendCorsPolicy.Configure(builder, ["http://localhost:5173"]);
        var policy = builder.Build();
        using var provider = new ServiceCollection().AddLogging().AddCors().BuildServiceProvider();
        var cors = provider.GetRequiredService<ICorsService>();

        var preflight = new DefaultHttpContext();
        preflight.Request.Method = HttpMethods.Options;
        preflight.Request.Headers.Origin = "http://localhost:5173";
        preflight.Request.Headers.AccessControlRequestMethod = HttpMethods.Post;
        preflight.Request.Headers.AccessControlRequestHeaders = "content-type,X-CSRF-TOKEN";
        var allowed = cors.EvaluatePolicy(preflight, policy);
        cors.ApplyResult(allowed, preflight.Response);

        Assert.True(allowed.IsOriginAllowed);
        Assert.True(allowed.IsPreflightRequest);
        Assert.True(allowed.SupportsCredentials);
        Assert.Equal("http://localhost:5173", preflight.Response.Headers.AccessControlAllowOrigin);
        Assert.Equal("true", preflight.Response.Headers.AccessControlAllowCredentials);
        Assert.NotEqual("*", preflight.Response.Headers.AccessControlAllowOrigin.ToString());
        Assert.Contains("POST", preflight.Response.Headers.AccessControlAllowMethods.ToString());
        Assert.Contains("X-CSRF-TOKEN", preflight.Response.Headers.AccessControlAllowHeaders.ToString(), StringComparison.OrdinalIgnoreCase);

        foreach (var method in new[] { HttpMethods.Get, HttpMethods.Post })
        {
            var request = new DefaultHttpContext();
            request.Request.Method = method;
            request.Request.Headers.Origin = "http://localhost:5173";
            var exactOrigin = cors.EvaluatePolicy(request, policy);
            cors.ApplyResult(exactOrigin, request.Response);
            Assert.True(exactOrigin.IsOriginAllowed);
            Assert.Equal("http://localhost:5173", request.Response.Headers.AccessControlAllowOrigin);
            Assert.Equal("true", request.Response.Headers.AccessControlAllowCredentials);
        }

        foreach (var method in new[] { HttpMethods.Get, HttpMethods.Post })
        {
            var request = new DefaultHttpContext();
            request.Request.Method = method;
            request.Request.Headers.Origin = "https://evil.example";
            var rejected = cors.EvaluatePolicy(request, policy);
            cors.ApplyResult(rejected, request.Response);
            Assert.False(rejected.IsOriginAllowed);
            Assert.False(request.Response.Headers.ContainsKey("Access-Control-Allow-Origin"));
        }
    }

    [Theory]
    [InlineData("*")]
    [InlineData("https://app.example/path")]
    [InlineData("not-an-origin")]
    public void CorsConfigurationRejectsWildcardPathsAndMalformedOrigins(string origin)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["Cors:AllowedOrigins:0"] = origin }).Build();

        Assert.Throws<InvalidOperationException>(() =>
            FrontendCorsPolicy.ReadAllowedOrigins(configuration));
    }

    private static ServiceProvider Services()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection().UseEphemeralDataProtectionProvider();
        services.AddAntiforgery(AuthAntiforgeryConfiguration.Configure);
        return services.BuildServiceProvider();
    }

    private static DefaultHttpContext Context(IServiceProvider services)
    {
        var context = new DefaultHttpContext { RequestServices = services };
        context.Request.Scheme = "https";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static (string Cookie, string RequestToken) IssueToken(
        IServiceProvider services,
        string? authenticationType = null)
    {
        var context = Context(services);
        if (authenticationType is not null)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "7")],
                authenticationType));
        }
        var tokens = services.GetRequiredService<IAntiforgery>().GetAndStoreTokens(context);
        var cookie = context.Response.Headers.SetCookie.ToString().Split(';', 2)[0];
        return (cookie, tokens.RequestToken!);
    }

    private static async Task<MiddlewareResult> InvokeMiddleware(
        IServiceProvider services,
        string method,
        string? authenticationType,
        string? cookie = null,
        string? requestToken = null,
        object? metadata = null,
        int? downstreamStatus = null)
    {
        var context = Context(services);
        context.Request.Method = method;
        if (authenticationType is not null)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, "7")],
                authenticationType));
        }
        if (cookie is not null) context.Request.Headers.Cookie = cookie;
        if (requestToken is not null)
            context.Request.Headers[AuthAntiforgeryConfiguration.HeaderName] = requestToken;
        if (metadata is not null)
        {
            context.SetEndpoint(new Endpoint(
                _ => Task.CompletedTask,
                new EndpointMetadataCollection(metadata),
                "test"));
        }

        var nextCalled = false;
        var middleware = new CookieAntiforgeryMiddleware(_ =>
        {
            nextCalled = true;
            if (downstreamStatus.HasValue)
                context.Response.StatusCode = downstreamStatus.Value;
            return Task.CompletedTask;
        });
        await middleware.InvokeAsync(context, services.GetRequiredService<IAntiforgery>());
        return new MiddlewareResult(context, nextCalled);
    }

    private static string RepoRoot() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private sealed record MiddlewareResult(DefaultHttpContext Context, bool NextCalled);
}
