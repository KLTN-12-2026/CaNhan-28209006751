using Atomos.Application.Common;
using Atomos.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Atomos.Application.Auth;

public sealed class AuthSessionService(
    IApplicationDbContext db,
    IApplicationDbTransactionFactory transactionFactory,
    IAuthSessionMutations mutations,
    AuthSessionOptions options,
    TimeProvider timeProvider) : IAuthSessionService
{
    public const int MaxRevokeReasonLength = 200;

    public async Task<CreatedAuthSession> CreateSessionAsync(
        int userId,
        AuthSessionMethod authMethod,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        if (!Enum.IsDefined(authMethod)) throw new ArgumentOutOfRangeException(nameof(authMethod));
        EnsureValidLifetime();

        await using var transaction = await transactionFactory.BeginAsync(cancellationToken);
        var userCanAuthenticate = await db.Users
            .AnyAsync(user => user.IDUser == userId && !user.DaKhoa && !user.DaXoa, cancellationToken);
        if (!userCanAuthenticate)
            throw new InvalidOperationException("Cannot create a session for a missing or unavailable user.");

        var now = UtcNow();
        var secret = AuthSessionSecret.Generate();
        var session = new AuthSession
        {
            IDUser = userId,
            SessionSecretHash = AuthSessionSecret.Hash(secret),
            AuthMethod = authMethod,
            CreatedAt = now,
            ExpiresAt = now.Add(options.Lifetime)
        };

        db.AuthSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new CreatedAuthSession(session.IDAuthSession, secret, session.ExpiresAt);
    }

    public async Task<ValidAuthSession?> GetValidSessionAsync(
        string rawSecret,
        CancellationToken cancellationToken = default)
    {
        if (!AuthSessionSecret.TryHash(rawSecret, out var hash)) return null;
        var now = UtcNow();
        var session = await db.AuthSessions
            .AsNoTracking()
            .Include(item => item.User)
            .ThenInclude(user => user!.Role)
            .SingleOrDefaultAsync(item =>
                item.SessionSecretHash.SequenceEqual(hash)
                && item.RevokedAt == null
                && item.ExpiresAt > now
                && item.User != null
                && !item.User.DaKhoa
                && !item.User.DaXoa,
                cancellationToken);

        return session is null
            ? null
            : new ValidAuthSession(
                session.IDAuthSession,
                session.User!,
                session.AuthMethod,
                session.CreatedAt,
                session.ExpiresAt);
    }

    public async Task RevokeSessionAsync(
        string rawSecret,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        if (!AuthSessionSecret.TryHash(rawSecret, out var hash)) return;
        reason = NormalizeReason(reason);
        var now = UtcNow();

        await mutations.RevokeSessionAsync(hash, now, reason, cancellationToken);
    }

    public async Task<int> RevokeAllUserSessionsAsync(
        int userId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
        reason = NormalizeReason(reason);
        var now = UtcNow();

        await using var transaction = await transactionFactory.BeginAsync(cancellationToken);
        var affected = await mutations.RevokeAllActiveUserSessionsAsync(
            userId, now, reason, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return affected;
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;

    private void EnsureValidLifetime()
    {
        if (options.LifetimeMinutes <= 0)
            throw new InvalidOperationException("AuthSession lifetime must be positive.");
    }

    private static string? NormalizeReason(string? reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) return null;
        reason = reason.Trim();
        if (reason.Length > MaxRevokeReasonLength)
            throw new ArgumentException($"Revoke reason cannot exceed {MaxRevokeReasonLength} characters.", nameof(reason));
        return reason;
    }
}
