using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Atomos.API.Security;

public static class FrontendCorsPolicy
{
    public const string Name = "Frontend";
    public const string SectionName = "Cors:AllowedOrigins";

    public static string[] ReadAllowedOrigins(IConfiguration configuration)
    {
        var origins = configuration.GetSection(SectionName).Get<string[]>() ?? [];
        var normalized = origins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var origin in normalized)
        {
            if (origin == "*"
                || !Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                || origin != uri.GetLeftPart(UriPartial.Authority))
            {
                throw new InvalidOperationException(
                    "Cors:AllowedOrigins must contain only exact HTTP(S) origins without paths or wildcards.");
            }
        }

        return normalized;
    }

    public static void Configure(CorsPolicyBuilder policy, IReadOnlyCollection<string> origins)
    {
        if (origins.Count > 0) policy.WithOrigins(origins.ToArray());
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    }
}
