using System.Security.Cryptography;

namespace Atomos.Application.Auth;

public static class AuthSessionSecret
{
    public const int SecretByteLength = 32;
    public const int HashByteLength = 32;

    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(SecretByteLength);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static byte[] Hash(string rawSecret)
    {
        if (!TryHash(rawSecret, out var hash))
            throw new ArgumentException("Session secret is not valid Base64URL data.", nameof(rawSecret));

        return hash;
    }

    public static bool TryHash(string? rawSecret, out byte[] hash)
    {
        hash = [];
        if (rawSecret is null
            || rawSecret.Length != 43
            || rawSecret.Any(character =>
                !char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_'))
            return false;

        try
        {
            var value = rawSecret.Replace('-', '+').Replace('_', '/');
            value += (value.Length % 4) switch
            {
                0 => string.Empty,
                2 => "==",
                3 => "=",
                _ => throw new FormatException()
            };
            var bytes = Convert.FromBase64String(value);
            if (bytes.Length != SecretByteLength) return false;
            hash = SHA256.HashData(bytes);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
