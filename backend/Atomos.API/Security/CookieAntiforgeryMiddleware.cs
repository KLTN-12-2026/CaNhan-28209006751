using Microsoft.AspNetCore.Antiforgery;

namespace Atomos.API.Security;

public sealed class CookieAntiforgeryMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> SafeMethods =
        new(StringComparer.OrdinalIgnoreCase)
        {
            HttpMethods.Get,
            HttpMethods.Head,
            HttpMethods.Options,
            HttpMethods.Trace
        };

    public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery)
    {
        if (!RequiresValidation(context))
        {
            await next(context);
            return;
        }

        try
        {
            await antiforgery.ValidateRequestAsync(context);
            await next(context);
        }
        catch (AntiforgeryValidationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsJsonAsync(
                new { message = "CSRF validation failed." },
                context.RequestAborted);
        }
    }

    public static bool RequiresValidation(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<SkipCookieAntiforgeryAttribute>() is not null)
            return false;

        var usesCookieAuthentication = context.User.Identities.Any(identity =>
            identity.IsAuthenticated
            && string.Equals(
                identity.AuthenticationType,
                AtomosAuthenticationDefaults.CookieScheme,
                StringComparison.Ordinal));
        if (!usesCookieAuthentication) return false;

        return !SafeMethods.Contains(context.Request.Method)
            || endpoint?.Metadata.GetMetadata<RequireCookieAntiforgeryAttribute>() is not null;
    }
}
