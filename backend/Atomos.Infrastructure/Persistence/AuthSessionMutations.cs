using Atomos.Application.Auth;
using Microsoft.EntityFrameworkCore;

namespace Atomos.Infrastructure.Persistence;

public sealed class AuthSessionMutations(AtomosDbContext db) : IAuthSessionMutations
{
    public async Task RevokeSessionAsync(
        byte[] secretHash,
        DateTime revokedAt,
        string? reason,
        CancellationToken cancellationToken)
    {
        await db.AuthSessions
            .Where(session => session.SessionSecretHash.SequenceEqual(secretHash)
                && session.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(session => session.RevokedAt, revokedAt)
                .SetProperty(session => session.RevokeReason, reason), cancellationToken);
    }

    public Task<int> RevokeAllActiveUserSessionsAsync(
        int userId,
        DateTime revokedAt,
        string? reason,
        CancellationToken cancellationToken) =>
        db.AuthSessions
            .Where(session => session.IDUser == userId
                && session.RevokedAt == null
                && session.ExpiresAt > revokedAt)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(session => session.RevokedAt, revokedAt)
                .SetProperty(session => session.RevokeReason, reason), cancellationToken);
}
