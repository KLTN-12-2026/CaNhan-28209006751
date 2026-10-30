using Microsoft.AspNetCore.Antiforgery;

namespace Atomos.API.Security;

public static class AuthAntiforgeryConfiguration
{
    public const string HeaderName = "X-CSRF-TOKEN";
    public const string CookieName = "__Host-Atomos.Csrf";

    public static void Configure(AntiforgeryOptions options)
    {
        options.HeaderName = HeaderName;
        options.Cookie.Name = CookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Path = "/";
        options.Cookie.Domain = null;
    }
}
