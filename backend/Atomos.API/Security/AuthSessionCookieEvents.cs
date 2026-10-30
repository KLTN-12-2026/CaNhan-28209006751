using System.Globalization;
using System.Security.Claims;
using Atomos.Application.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Atomos.API.Security;

public sealed class AuthSessionCookieEvents(
    IAuthSessionService sessions,
    ILogger<AuthSessionCookieEvents> logger) : CookieAuthenticationEvents
{
    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var handle = context.Principal?.FindFirstValue(
            OpaqueSessionTicketDataFormat.SessionHandleClaimType);
        if (!AuthSessionSecret.TryHash(handle, out _))
        {
            context.RejectPrincipal();
            return;
        }

        try
        {
            var session = await sessions.GetValidSessionAsync(
                handle!, context.HttpContext.RequestAborted);
            if (session?.User.Role is null
                || string.IsNullOrWhiteSpace(session.User.Role.TenRole))
            {
                context.RejectPrincipal();
                return;
            }

            context.ReplacePrincipal(BuildPrincipal(session));
        }
        catch (OperationCanceledException) when (context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            logger.LogWarning("Cookie session resolution failed closed.");
            context.RejectPrincipal();
        }
    }

    internal static ClaimsPrincipal BuildPrincipal(ValidAuthSession session)
    {
        var user = session.User;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.IDUser.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role!.TenRole)
        };

        if (!string.IsNullOrWhiteSpace(user.HoTen))
            claims.Add(new Claim("full_name", user.HoTen));
        if (!string.IsNullOrWhiteSpace(user.Avatar))
            claims.Add(new Claim("avatar", user.Avatar));

        return new ClaimsPrincipal(new ClaimsIdentity(
            claims,
            AtomosAuthenticationDefaults.CookieScheme,
            ClaimTypes.Name,
            ClaimTypes.Role));
    }
}
