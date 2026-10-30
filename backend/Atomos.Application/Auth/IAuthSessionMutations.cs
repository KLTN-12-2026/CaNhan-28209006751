namespace Atomos.Application.Auth;

public interface IAuthSessionMutations
{
    Task RevokeSessionAsync(
        byte[] secretHash,
        DateTime revokedAt,
        string? reason,
        CancellationToken cancellationToken);

    Task<int> RevokeAllActiveUserSessionsAsync(
        int userId,
        DateTime revokedAt,
        string? reason,
        CancellationToken cancellationToken);
}
