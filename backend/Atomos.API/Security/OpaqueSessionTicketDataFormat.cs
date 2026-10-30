using System.Security.Claims;
using Atomos.Application.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

namespace Atomos.API.Security;

public sealed class OpaqueSessionTicketDataFormat : ISecureDataFormat<AuthenticationTicket>
{
    internal const string SessionHandleClaimType = "atomos:session-handle";

    public string Protect(AuthenticationTicket data) => Protect(data, purpose: null);

    public string Protect(AuthenticationTicket data, string? purpose)
    {
        var handle = data.Principal.FindFirstValue(SessionHandleClaimType);
        if (!AuthSessionSecret.TryHash(handle, out _))
            throw new InvalidOperationException("A canonical opaque session handle is required.");

        return handle!;
    }

    public AuthenticationTicket? Unprotect(string? protectedText) =>
        Unprotect(protectedText, purpose: null);

    public AuthenticationTicket? Unprotect(string? protectedText, string? purpose)
    {
        if (!AuthSessionSecret.TryHash(protectedText, out _)) return null;

        var identity = new ClaimsIdentity(
            [new Claim(SessionHandleClaimType, protectedText!)],
            AtomosAuthenticationDefaults.CookieScheme);
        return new AuthenticationTicket(
            new ClaimsPrincipal(identity),
            new AuthenticationProperties(),
            AtomosAuthenticationDefaults.CookieScheme);
    }
}
