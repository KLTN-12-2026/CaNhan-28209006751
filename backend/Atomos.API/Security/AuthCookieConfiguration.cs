using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace Atomos.API.Security;

public static class AuthCookieConfiguration
{
    public static void Configure(CookieAuthenticationOptions options, TimeSpan lifetime)
    {
        options.Cookie.Name = AtomosAuthenticationDefaults.CookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Path = "/";
        options.Cookie.Domain = null;
        options.ExpireTimeSpan = lifetime;
        options.SlidingExpiration = false;
        options.EventsType = typeof(AuthSessionCookieEvents);
        options.TicketDataFormat = new OpaqueSessionTicketDataFormat();
    }
}
