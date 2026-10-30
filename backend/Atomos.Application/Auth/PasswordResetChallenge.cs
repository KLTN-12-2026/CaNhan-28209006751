using System.Security.Cryptography;
using System.Text;
using Atomos.Domain.Users;

namespace Atomos.Application.Auth;

// Mutations must be persisted while holding the per-user password-reset transaction.
public static class PasswordResetChallenge
{
    public const int MaxFailedAttempts = 5;

    public static bool IsOtp(string value) => value.Length == 6 && value.All(char.IsAsciiDigit);
    public static bool IsGrant(string value) => value.Length == 64 && value.All(char.IsAsciiHexDigit);

    public static bool TryAuthenticate(PasswordResetToken token, string secret, DateTime now, bool otpOnly)
    {
        if (token.DaSuDung || token.HetHan <= now || token.FailedAttempts >= MaxFailedAttempts)
            return false;

        var isOtp = IsOtp(token.Token);
        if (otpOnly && !isOtp) return false;
        var candidate = isOtp ? secret : HashGrant(secret);
        if ((isOtp ? IsOtp(secret) : IsGrant(secret)) &&
            CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(token.Token), Encoding.UTF8.GetBytes(candidate)))
            return true;

        token.FailedAttempts++;
        if (token.FailedAttempts >= MaxFailedAttempts) token.DaSuDung = true;
        return false;
    }

    public static string ExchangeOtpForGrant(PasswordResetToken token)
    {
        var grant = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        // Replacing the OTP makes it unusable immediately. Only the grant hash is stored.
        token.Token = HashGrant(grant);
        return grant;
    }

    private static string HashGrant(string grant) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(grant)));
}
