using Atomos.Application.Auth;
using Atomos.Domain.Users;
using Atomos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atomos.Tests;

public sealed class AuthSessionTests
{
    private static string ConnectionString => SqlIntegrationTestDatabase.RequireConnectionString();
    private static readonly DateTimeOffset TestNow = new(2026, 10, 5, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SecretsUseThirtyTwoRandomBytesAndDeterministicSha256()
    {
        var first = AuthSessionSecret.Generate();
        var second = AuthSessionSecret.Generate();

        Assert.NotEqual(first, second);
        Assert.Equal(43, first.Length);
        Assert.DoesNotContain('=', first);
        Assert.Equal(AuthSessionSecret.Hash(first), AuthSessionSecret.Hash(first));
        Assert.NotEqual(AuthSessionSecret.Hash(first), AuthSessionSecret.Hash(second));
        Assert.Equal(AuthSessionSecret.HashByteLength, AuthSessionSecret.Hash(first).Length);
        Assert.False(AuthSessionSecret.TryHash("not-a-session-secret", out _));
        Assert.False(AuthSessionSecret.TryHash(first + "=", out _));
        Assert.False(AuthSessionSecret.TryHash(new string('A', 42) + "+", out _));
    }

    [Fact]
    public async Task CreatePersistsOnlyHashAndLookupUsesCurrentUserAndRole()
    {
        await using var db = CreateDb();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await CreateUser(db);
        var clock = new TestTimeProvider(TestNow);
        var service = Service(db, clock);

        var created = await service.CreateSessionAsync(user.IDUser, AuthSessionMethod.Password);
        var stored = await db.AuthSessions.AsNoTracking().SingleAsync(item => item.IDAuthSession == created.SessionId);
        var valid = await service.GetValidSessionAsync(created.Secret);

        Assert.Equal(TestNow.UtcDateTime.AddMinutes(60), created.ExpiresAt);
        Assert.Equal(AuthSessionSecret.Hash(created.Secret), stored.SessionSecretHash);
        Assert.Equal(AuthSessionSecret.HashByteLength, stored.SessionSecretHash.Length);
        Assert.Equal(AuthSessionMethod.Password, stored.AuthMethod);
        Assert.Equal(TestNow.UtcDateTime, stored.CreatedAt);
        Assert.NotNull(valid);
        Assert.Equal(user.IDUser, valid!.User.IDUser);
        Assert.Equal("Student", valid.User.Role!.TenRole);
        Assert.Null(await service.GetValidSessionAsync(AuthSessionSecret.Generate()));
        Assert.DoesNotContain(typeof(AuthSession).GetProperties(), property =>
            property.PropertyType == typeof(string)
            && property.Name.Contains("Secret", StringComparison.OrdinalIgnoreCase));
        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task LookupFailsClosedAtExpirationBoundaryAndForUnavailableUsers()
    {
        await using var db = CreateDb();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await CreateUser(db);
        var clock = new TestTimeProvider(TestNow);
        var service = Service(db, clock);
        var created = await service.CreateSessionAsync(user.IDUser, AuthSessionMethod.Google);

        clock.Set(created.ExpiresAt);
        Assert.Null(await service.GetValidSessionAsync(created.Secret));

        clock.Set(TestNow.UtcDateTime);
        user.DaKhoa = true;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Null(await service.GetValidSessionAsync(created.Secret));

        var storedUser = await db.Users.SingleAsync(item => item.IDUser == user.IDUser);
        storedUser.DaKhoa = false;
        storedUser.DaXoa = true;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Null(await service.GetValidSessionAsync(created.Secret));
        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task LookupUsesCurrentRoleInsteadOfSessionCreationSnapshot()
    {
        await using var db = CreateDb();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await CreateUser(db, roleName: "Admin");
        var service = Service(db, new TestTimeProvider(TestNow));
        var created = await service.CreateSessionAsync(user.IDUser, AuthSessionMethod.Password);
        var studentRoleId = await db.Roles.Where(role => role.TenRole == "Student")
            .Select(role => role.IDRole)
            .SingleAsync();

        user.IDRole = studentRoleId;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var resolved = await service.GetValidSessionAsync(created.Secret);

        Assert.NotNull(resolved);
        Assert.Equal("Student", resolved!.User.Role!.TenRole);
        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task CreateRejectsMissingLockedAndDeletedUsers()
    {
        await using var db = CreateDb();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var clock = new TestTimeProvider(TestNow);
        var service = Service(db, clock);
        var locked = await CreateUser(db, locked: true);
        var deleted = await CreateUser(db, deleted: true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateSessionAsync(int.MaxValue, AuthSessionMethod.Password));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateSessionAsync(locked.IDUser, AuthSessionMethod.Password));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateSessionAsync(deleted.IDUser, AuthSessionMethod.Google));
        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task RevokeIsIdempotentPreservesFirstReasonAndRejectsReplay()
    {
        await using var db = CreateDb();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await CreateUser(db);
        var service = Service(db, new TestTimeProvider(TestNow));
        var created = await service.CreateSessionAsync(user.IDUser, AuthSessionMethod.Password);

        await service.RevokeSessionAsync(created.Secret, "first reason");
        await service.RevokeSessionAsync(created.Secret, "must not overwrite");
        await service.RevokeSessionAsync("invalid", "ignored");
        db.ChangeTracker.Clear();
        var stored = await db.AuthSessions.SingleAsync(item => item.IDAuthSession == created.SessionId);

        Assert.Equal(TestNow.UtcDateTime, stored.RevokedAt);
        Assert.Equal("first reason", stored.RevokeReason);
        Assert.Null(await service.GetValidSessionAsync(created.Secret));
        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task RevokeAllOnlyMutatesActiveSessionsForRequestedUser()
    {
        await using var db = CreateDb();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var firstUser = await CreateUser(db);
        var secondUser = await CreateUser(db);
        var clock = new TestTimeProvider(TestNow);
        var service = Service(db, clock);
        var first = await service.CreateSessionAsync(firstUser.IDUser, AuthSessionMethod.Password);
        var second = await service.CreateSessionAsync(firstUser.IDUser, AuthSessionMethod.Google);
        var other = await service.CreateSessionAsync(secondUser.IDUser, AuthSessionMethod.Password);
        var expired = new AuthSession
        {
            IDUser = firstUser.IDUser,
            SessionSecretHash = AuthSessionSecret.Hash(AuthSessionSecret.Generate()),
            AuthMethod = AuthSessionMethod.Password,
            CreatedAt = TestNow.UtcDateTime.AddHours(-2),
            ExpiresAt = TestNow.UtcDateTime.AddHours(-1)
        };
        db.AuthSessions.Add(expired);
        await db.SaveChangesAsync();

        Assert.Equal(2, await service.RevokeAllUserSessionsAsync(firstUser.IDUser, "account logout"));
        Assert.Equal(0, await service.RevokeAllUserSessionsAsync(firstUser.IDUser, "second call"));
        Assert.Null(await service.GetValidSessionAsync(first.Secret));
        Assert.Null(await service.GetValidSessionAsync(second.Secret));
        Assert.NotNull(await service.GetValidSessionAsync(other.Secret));
        db.ChangeTracker.Clear();
        var expiredStored = await db.AuthSessions.SingleAsync(item => item.IDAuthSession == expired.IDAuthSession);
        Assert.Null(expiredStored.RevokedAt);
        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task ConcurrentRevocationIsIdempotentAndPostRevokeLookupFails()
    {
        int userId;
        CreatedAuthSession created;
        await using (var setup = CreateDb())
        {
            var user = await CreateUser(setup);
            userId = user.IDUser;
            created = await Service(setup, new TestTimeProvider(TestNow))
                .CreateSessionAsync(userId, AuthSessionMethod.Password);
        }

        try
        {
            var attempts = new[] { "reason-a", "reason-b" }.Select(async reason =>
            {
                await using var db = CreateDb();
                await Service(db, new TestTimeProvider(TestNow))
                    .RevokeSessionAsync(created.Secret, reason);
            });
            await Task.WhenAll(attempts);

            await using var verify = CreateDb();
            var stored = await verify.AuthSessions.AsNoTracking()
                .SingleAsync(item => item.IDAuthSession == created.SessionId);
            Assert.Contains(stored.RevokeReason, new[] { "reason-a", "reason-b" });
            Assert.Null(await Service(verify, new TestTimeProvider(TestNow))
                .GetValidSessionAsync(created.Secret));
        }
        finally
        {
            await using var cleanup = CreateDb();
            await cleanup.Users.Where(item => item.IDUser == userId).ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task ConcurrentRevokeAllCallsAreAtomicAndDoNotAffectOtherUsers()
    {
        int firstUserId;
        int otherUserId;
        CreatedAuthSession otherSession;
        await using (var setup = CreateDb())
        {
            var firstUser = await CreateUser(setup);
            var otherUser = await CreateUser(setup);
            firstUserId = firstUser.IDUser;
            otherUserId = otherUser.IDUser;
            var service = Service(setup, new TestTimeProvider(TestNow));
            await service.CreateSessionAsync(firstUserId, AuthSessionMethod.Password);
            await service.CreateSessionAsync(firstUserId, AuthSessionMethod.Google);
            otherSession = await service.CreateSessionAsync(otherUserId, AuthSessionMethod.Password);
        }

        try
        {
            var calls = new[] { "all-a", "all-b" }.Select(async reason =>
            {
                await using var db = CreateDb();
                return await Service(db, new TestTimeProvider(TestNow))
                    .RevokeAllUserSessionsAsync(firstUserId, reason);
            });
            var affected = await Task.WhenAll(calls);

            Assert.Equal(2, affected.Sum());
            await using var verify = CreateDb();
            Assert.Empty(await verify.AuthSessions.AsNoTracking()
                .Where(item => item.IDUser == firstUserId && item.RevokedAt == null)
                .ToListAsync());
            Assert.NotNull(await Service(verify, new TestTimeProvider(TestNow))
                .GetValidSessionAsync(otherSession.Secret));
        }
        finally
        {
            await using var cleanup = CreateDb();
            await cleanup.Users.Where(item => item.IDUser == firstUserId || item.IDUser == otherUserId)
                .ExecuteDeleteAsync();
        }
    }

    [Fact]
    public async Task CreateAndRevokeAllFollowDatabaseStatementOrdering()
    {
        await using var db = CreateDb();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await CreateUser(db);
        var service = Service(db, new TestTimeProvider(TestNow));

        var before = await service.CreateSessionAsync(user.IDUser, AuthSessionMethod.Password);
        Assert.Equal(1, await service.RevokeAllUserSessionsAsync(user.IDUser, "rotate"));
        var after = await service.CreateSessionAsync(user.IDUser, AuthSessionMethod.Password);

        Assert.Null(await service.GetValidSessionAsync(before.Secret));
        Assert.NotNull(await service.GetValidSessionAsync(after.Secret));
        await transaction.RollbackAsync();
    }

    private static AuthSessionService Service(AtomosDbContext db, TimeProvider timeProvider) =>
        new(
            db,
            new EfApplicationDbTransactionFactory(db),
            new AuthSessionMutations(db),
            new AuthSessionOptions { LifetimeMinutes = 60 },
            timeProvider);

    private static AtomosDbContext CreateDb() => new(new DbContextOptionsBuilder<AtomosDbContext>()
        .UseSqlServer(ConnectionString)
        .Options);

    private static async Task<User> CreateUser(
        AtomosDbContext db,
        bool locked = false,
        bool deleted = false,
        string roleName = "Student")
    {
        var marker = Guid.NewGuid().ToString("N");
        var user = new User
        {
            IDRole = await db.Roles.Where(role => role.TenRole == roleName)
                .Select(role => role.IDRole)
                .SingleAsync(),
            Username = $"session_{marker}"[..40],
            Email = $"session_{marker}@example.test",
            DaKhoa = locked,
            DaXoa = deleted,
            NgayTao = TestNow.UtcDateTime,
            NgayCapNhat = TestNow.UtcDateTime
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private sealed class TestTimeProvider(DateTimeOffset value) : TimeProvider
    {
        private DateTimeOffset value = value;
        public override DateTimeOffset GetUtcNow() => value;
        public void Set(DateTime value) => this.value = new DateTimeOffset(value, TimeSpan.Zero);
    }
}
