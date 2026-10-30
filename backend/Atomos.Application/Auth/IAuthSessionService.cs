using Atomos.Domain.Users;

namespace Atomos.Application.Auth;

public interface IAuthSessionService
{
    Task<CreatedAuthSession> CreateSessionAsync(
        int userId,
        AuthSessionMethod authMethod,
        CancellationToken cancellationToken = default);

    Task<ValidAuthSession?> GetValidSessionAsync(
        string rawSecret,
        CancellationToken cancellationToken = default);

    Task RevokeSessionAsync(
        string rawSecret,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task<int> RevokeAllUserSessionsAsync(
        int userId,
        string? reason = null,
        CancellationToken cancellationToken = default);
}

public sealed record CreatedAuthSession(long SessionId, string Secret, DateTime ExpiresAt);

public sealed record ValidAuthSession(
    long SessionId,
    User User,
    AuthSessionMethod AuthMethod,
    DateTime CreatedAt,
    DateTime ExpiresAt);
