using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Atomos.Application.Common;

namespace Atomos.API.Security;

public sealed class GoogleTokenVerifier(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ILogger<GoogleTokenVerifier> logger) : IGoogleTokenVerifier
{
    private const string GoogleKeysUrl = "https://www.googleapis.com/oauth2/v3/certs";
    private static readonly string[] ValidIssuers = ["https://accounts.google.com", "accounts.google.com"];

    public async Task<GoogleUserInfo?> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        var clientId = configuration["Authentication:Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
        {
            logger.LogWarning("Google ClientId is not configured.");
            return null;
        }

        try
        {
            return await VerifyTokenAsync(idToken, clientId, cancellationToken);
        }
        catch (Exception exception) when (
            exception is FormatException or JsonException or KeyNotFoundException
            or InvalidOperationException or ArgumentException or CryptographicException)
        {
            logger.LogInformation("Rejected malformed Google identity token.");
            return null;
        }
    }

    private async Task<GoogleUserInfo?> VerifyTokenAsync(
        string idToken,
        string clientId,
        CancellationToken cancellationToken)
    {
        var parts = idToken.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        using var headerDocument = JsonDocument.Parse(Base64UrlDecode(parts[0]));
        var header = headerDocument.RootElement;
        var kid = header.GetProperty("kid").GetString();
        var alg = header.GetProperty("alg").GetString();
        if (string.IsNullOrWhiteSpace(kid) || alg != "RS256")
        {
            return null;
        }

        using var payloadDocument = JsonDocument.Parse(Base64UrlDecode(parts[1]));
        var payload = payloadDocument.RootElement;
        if (!IsPayloadValid(payload, clientId))
        {
            return null;
        }

        if (!await VerifySignatureAsync(kid, $"{parts[0]}.{parts[1]}", parts[2], cancellationToken))
        {
            return null;
        }

        return new GoogleUserInfo
        {
            GoogleId = payload.GetProperty("sub").GetString() ?? string.Empty,
            Email = payload.GetProperty("email").GetString() ?? string.Empty,
            FullName = payload.TryGetProperty("name", out var name) ? name.GetString() : null,
            Avatar = payload.TryGetProperty("picture", out var picture) ? picture.GetString() : null
        };
    }

    private static bool IsPayloadValid(JsonElement payload, string clientId)
    {
        var issuer = payload.GetProperty("iss").GetString();
        var audience = payload.GetProperty("aud").GetString();
        var expiresAt = payload.GetProperty("exp").GetInt64();
        var emailVerified = payload.TryGetProperty("email_verified", out var verified) &&
            verified.ValueKind == JsonValueKind.True;

        return ValidIssuers.Contains(issuer) &&
            audience == clientId &&
            DateTimeOffset.FromUnixTimeSeconds(expiresAt) > DateTimeOffset.UtcNow &&
            emailVerified;
    }

    private async Task<bool> VerifySignatureAsync(
        string kid,
        string signedPayload,
        string signature,
        CancellationToken cancellationToken)
    {
        using var keysDocument = await JsonDocument.ParseAsync(
            await httpClientFactory.CreateClient().GetStreamAsync(GoogleKeysUrl, cancellationToken),
            cancellationToken: cancellationToken);

        foreach (var key in keysDocument.RootElement.GetProperty("keys").EnumerateArray())
        {
            if (key.GetProperty("kid").GetString() != kid)
            {
                continue;
            }

            using var rsa = RSA.Create();
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = Base64UrlDecode(key.GetProperty("n").GetString() ?? string.Empty),
                Exponent = Base64UrlDecode(key.GetProperty("e").GetString() ?? string.Empty)
            });

            return rsa.VerifyData(
                Encoding.ASCII.GetBytes(signedPayload),
                Base64UrlDecode(signature),
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }

        return false;
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var paddedValue = value.Replace('-', '+').Replace('_', '/');
        paddedValue = paddedValue.PadRight(paddedValue.Length + (4 - paddedValue.Length % 4) % 4, '=');
        return Convert.FromBase64String(paddedValue);
    }
}
