using System.Security.Claims;
using Atomos.Application.Auth;
using Atomos.Domain.Users;
using Microsoft.AspNetCore.Authentication;

namespace Atomos.API.Security;

public sealed class AuthSessionCookieIssuer(IAuthSessionService sessions)
{
    public async Task IssueAsync(
        HttpContext httpContext,
        int userId,
        AuthSessionMethod authMethod,
        CancellationToken cancellationToken)
    {
        var session = await sessions.CreateSessionAsync(userId, authMethod, cancellationToken);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(OpaqueSessionTicketDataFormat.SessionHandleClaimType, session.Secret)],
            AtomosAuthenticationDefaults.CookieScheme));

        await httpContext.SignInAsync(
            AtomosAuthenticationDefaults.CookieScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = false
            });
    }
}
