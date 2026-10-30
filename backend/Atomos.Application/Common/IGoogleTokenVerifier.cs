namespace Atomos.Application.Common;

public interface IGoogleTokenVerifier
{
    Task<GoogleUserInfo?> VerifyAsync(string idToken, CancellationToken cancellationToken);
}
