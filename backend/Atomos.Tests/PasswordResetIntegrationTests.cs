using System.Security.Cryptography;
using Atomos.Application.Auth;
using Atomos.Application.Common;
using Atomos.Domain.Users;
using Atomos.Infrastructure.Persistence;
using Atomos.Infrastructure.Queries;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VerifyResetCodeResult = Atomos.Application.Common.Result<Atomos.Application.Auth.VerifyResetCodeResponse>;
using ResetPasswordResult = Atomos.Application.Common.Result<Atomos.Application.Auth.ResetPasswordResponse>;

namespace Atomos.Tests;

// Real SQL only. Never falls back to application secrets or the developer database.
public sealed class PasswordResetIntegrationTests : IAsyncLifetime
{
    private string connectionString = string.Empty;
    private readonly List<int> userIds = [];
    private readonly NoMail emailSender = new();

    public async Task InitializeAsync()
    {
        connectionString = Environment.GetEnvironmentVariable("ATOMOS_TEST_CONNECTION") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("SQL integration BLOCKED: set ATOMOS_TEST_CONNECTION to a bootstrapped disposable Atomos_OtpTest_* or Atomos_CI_* database.");
        var database = new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        if (!database.StartsWith("Atomos_OtpTest_", StringComparison.OrdinalIgnoreCase) &&
            !database.StartsWith("Atomos_CI_", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("SQL integration BLOCKED: refusing a non-disposable database.");
        await using var db = CreateDb();
        // Fail before any fixture writes if the required upgrade was not applied.
        await db.PasswordResetTokens.AsNoTracking().Select(token => token.FailedAttempts).Take(1).ToListAsync();
    }

    public async Task DisposeAsync()
    {
        if (userIds.Count == 0) return;
        await using var db = CreateDb();
        await db.Users.Where(user => userIds.Contains(user.IDUser)).ExecuteDeleteAsync();
    }

    [Fact]
    public async Task LifecyclePreservesExpiryResendIsolationAndSingleUse()
    {
        var a = await CreateUserAsync();
        var b = await CreateUserAsync();
        var old = await CreateTokenAsync(a.IDUser);
        await CreateTokenAsync(b.IDUser);
        var wrong = old.Token == "000000" ? "000001" : "000000";
        for (var i = 0; i < 5; i++)
        {
            var result = await WithService(service => service.VerifyResetCodeAsync(new()
            {
                UsernameOrEmail = i % 2 == 0 ? a.Username.ToUpperInvariant() : $" {a.Email} ", Code = wrong
            }, default));
            Assert.False(result.Success);
            Assert.Equal("RESET_CODE_INVALID", result.Code);
        }
        await using (var db = CreateDb())
        {
            var locked = await db.PasswordResetTokens.SingleAsync(token => token.IDReset == old.IDReset);
            Assert.Equal(5, locked.FailedAttempts);
            Assert.True(locked.DaSuDung);
            Assert.Equal(0, (await db.PasswordResetTokens.SingleAsync(token => token.IDUser == b.IDUser)).FailedAttempts);
            locked.NgayTao = DateTime.Now.AddMinutes(-2);
            await db.SaveChangesAsync();
        }
        Assert.False((await Verify(a, old.Token)).Success);
        Assert.False((await Reset(a, old.Token)).Success);
        Assert.True((await WithService(service => service.ForgotPasswordAsync(new() { UsernameOrEmail = a.Username }, default))).Success);
        PasswordResetToken fresh;
        await using (var db = CreateDb())
        {
            fresh = await db.PasswordResetTokens.AsNoTracking().Where(token => token.IDUser == a.IDUser).OrderByDescending(token => token.IDReset).FirstAsync();
            Assert.NotEqual(old.IDReset, fresh.IDReset);
            Assert.Equal(0, fresh.FailedAttempts);
        }
        var verified = await Verify(a, fresh.Token);
        Assert.True(verified.Success);
        Assert.False((await Verify(a, fresh.Token)).Success);
        Assert.False((await Reset(a, fresh.Token)).Success);
        var grant = verified.Data!.ResetToken;
        Assert.True((await Reset(a, grant)).Success);
        Assert.False((await Reset(a, grant)).Success);
        await using (var db = CreateDb())
        {
            Assert.True(PasswordHasher.Verify("NewPassword123!", (await db.Users.SingleAsync(user => user.IDUser == a.IDUser)).MatKhau!));
        }
        await using (var db = CreateDb())
        {
            var directOtp = await db.PasswordResetTokens.AsNoTracking().SingleAsync(token => token.IDUser == b.IDUser);
            Assert.True((await Reset(b, directOtp.Token)).Success);
            Assert.False((await Reset(b, directOtp.Token)).Success);
        }
        var expired = await CreateTokenAsync(b.IDUser, expired: true);
        Assert.False((await Verify(b, expired.Token)).Success);
        Assert.False((await Reset(b, expired.Token)).Success);
    }

    [Fact]
    public async Task SuccessfulPasswordResetRevokesOnlyActiveSessionsForThatUserAtomically()
    {
        var user = await CreateUserAsync();
        var other = await CreateUserAsync();
        CreatedAuthSession current;
        CreatedAuthSession unrelated;
        long expiredId;
        await using (var db = CreateDb())
        {
            var sessions = SessionService(db);
            current = await sessions.CreateSessionAsync(user.IDUser, AuthSessionMethod.Password);
            unrelated = await sessions.CreateSessionAsync(other.IDUser, AuthSessionMethod.Google);
            var expired = new AuthSession
            {
                IDUser = user.IDUser,
                SessionSecretHash = AuthSessionSecret.Hash(AuthSessionSecret.Generate()),
                AuthMethod = AuthSessionMethod.Password,
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                ExpiresAt = DateTime.UtcNow.AddHours(-1)
            };
            db.AuthSessions.Add(expired);
            await db.SaveChangesAsync();
            expiredId = expired.IDAuthSession;
        }
        var token = await CreateTokenAsync(user.IDUser);

        var result = await Reset(user, token.Token);

        Assert.True(result.Success);
        await using var verify = CreateDb();
        var currentStored = await verify.AuthSessions.AsNoTracking()
            .SingleAsync(session => session.IDAuthSession == current.SessionId);
        Assert.NotNull(currentStored.RevokedAt);
        Assert.Equal(AuthSessionRevokeReasons.PasswordReset, currentStored.RevokeReason);
        Assert.Null((await verify.AuthSessions.AsNoTracking()
            .SingleAsync(session => session.IDAuthSession == expiredId)).RevokedAt);
        var sessionService = SessionService(verify);
        Assert.Null(await sessionService.GetValidSessionAsync(current.Secret));
        Assert.NotNull(await sessionService.GetValidSessionAsync(unrelated.Secret));
    }

    [Fact]
    public async Task ConcurrentVerifyAndDirectResetShareAnAtomicBudget()
    {
        var user = await CreateUserAsync();
        var token = await CreateTokenAsync(user.IDUser);
        var wrong = token.Token == "000000" ? "000001" : "000000";
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var requests = Enumerable.Range(0, 20).Select(async i =>
        {
            await gate.Task;
            return i % 2 == 0 ? (await Verify(user, wrong)).Success : (await Reset(user, wrong)).Success;
        }).ToArray();
        gate.SetResult();
        Assert.All(await Task.WhenAll(requests), success => Assert.False(success));
        await using var db = CreateDb();
        var stored = await db.PasswordResetTokens.AsNoTracking().SingleAsync(item => item.IDReset == token.IDReset);
        Assert.Equal(5, stored.FailedAttempts);
        Assert.True(stored.DaSuDung);
        Assert.False((await Verify(user, token.Token)).Success);
        Assert.False((await Reset(user, token.Token)).Success);
    }

    [Fact]
    public async Task ConcurrentSuccessfulVerificationAndResetHaveOneWinner()
    {
        var user = await CreateUserAsync();
        var token = await CreateTokenAsync(user.IDUser);
        var verifications = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Verify(user, token.Token)));
        var grant = Assert.Single(verifications, result => result.Success).Data!.ResetToken;
        var resets = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Reset(user, grant)));
        Assert.Single(resets, result => result.Success);
    }

    [Fact]
    public async Task ConcurrentResendHonorsCooldownAndInvalidatesOldOtp()
    {
        var user = await CreateUserAsync();
        var old = await CreateTokenAsync(user.IDUser);
        await using (var db = CreateDb())
        {
            var stored = await db.PasswordResetTokens.SingleAsync(token => token.IDReset == old.IDReset);
            stored.NgayTao = DateTime.Now.AddMinutes(-2);
            await db.SaveChangesAsync();
        }
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => WithService(service =>
            service.ForgotPasswordAsync(new() { UsernameOrEmail = user.Username }, default))));
        await using (var db = CreateDb())
        {
            var tokens = await db.PasswordResetTokens.AsNoTracking().Where(token => token.IDUser == user.IDUser).ToListAsync();
            Assert.Equal(2, tokens.Count);
            Assert.True(tokens.Single(token => token.IDReset == old.IDReset).DaSuDung);
            Assert.Single(tokens, token => !token.DaSuDung);
        }
        Assert.False((await Verify(user, old.Token)).Success);
        await using (var db = CreateDb())
        {
            var tokens = await db.PasswordResetTokens.Where(token => token.IDUser == user.IDUser).ToListAsync();
            foreach (var token in tokens) token.NgayTao = DateTime.Now.AddMinutes(-2);
            for (var i = tokens.Count; i < 9; i++)
                db.PasswordResetTokens.Add(new PasswordResetToken
                {
                    IDUser = user.IDUser, Token = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6"),
                    NgayTao = DateTime.Now.AddMinutes(-3), HetHan = DateTime.Now.AddMinutes(27), DaSuDung = true
                });
            await db.SaveChangesAsync();
        }
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => WithService(service =>
            service.ForgotPasswordAsync(new() { UsernameOrEmail = user.Username }, default))));
        await using (var db = CreateDb())
        {
            var tokens = await db.PasswordResetTokens.Where(token => token.IDUser == user.IDUser).ToListAsync();
            Assert.Equal(10, tokens.Count);
            Assert.Single(tokens, token => !token.DaSuDung);
            foreach (var token in tokens) token.NgayTao = DateTime.Now.AddMinutes(-2);
            await db.SaveChangesAsync();
        }
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => WithService(service =>
            service.ForgotPasswordAsync(new() { UsernameOrEmail = user.Username }, default))));
        await using (var db = CreateDb())
            Assert.Equal(10, await db.PasswordResetTokens.CountAsync(token => token.IDUser == user.IDUser));
    }

    [Fact]
    public async Task ForgotPassword_DoesNotRevealWhetherAccountExists()
    {
        var user = await CreateUserAsync();
        var existing = await WithService(service => service.ForgotPasswordAsync(
            new() { UsernameOrEmail = user.Username }, default));
        var missing = await WithService(service => service.ForgotPasswordAsync(
            new() { UsernameOrEmail = $"missing{Guid.NewGuid():N}"[..30] }, default));

        Assert.True(existing.Success);
        Assert.True(missing.Success);
        Assert.Equal(existing.Code, missing.Code);
        Assert.Equal(existing.Message, missing.Message);
        Assert.Equal(existing.Data!.Message, missing.Data!.Message);
    }

    [Fact]
    public async Task LockedAndDeletedAccountsCannotResetWithoutObservableStateDifference()
    {
        var locked = await CreateUserAsync();
        var deleted = await CreateUserAsync();
        await using (var db = CreateDb())
        {
            (await db.Users.SingleAsync(user => user.IDUser == locked.IDUser)).DaKhoa = true;
            (await db.Users.SingleAsync(user => user.IDUser == deleted.IDUser)).DaXoa = true;
            await db.SaveChangesAsync();
        }

        var lockedResult = await WithService(service => service.ForgotPasswordAsync(
            new() { UsernameOrEmail = locked.Username }, default));
        var deletedResult = await WithService(service => service.ForgotPasswordAsync(
            new() { UsernameOrEmail = deleted.Username }, default));
        var missingResult = await WithService(service => service.ForgotPasswordAsync(
            new() { UsernameOrEmail = $"missing{Guid.NewGuid():N}"[..30] }, default));

        Assert.Equal((missingResult.Success, missingResult.Code, missingResult.Message),
            (lockedResult.Success, lockedResult.Code, lockedResult.Message));
        Assert.Equal((missingResult.Success, missingResult.Code, missingResult.Message),
            (deletedResult.Success, deletedResult.Code, deletedResult.Message));
        await using (var db = CreateDb())
            Assert.False(await db.PasswordResetTokens.AnyAsync(token =>
                token.IDUser == locked.IDUser || token.IDUser == deleted.IDUser));

        var token = await CreateTokenAsync(locked.IDUser);
        var reset = await Reset(locked, token.Token);
        Assert.False(reset.Success);
        await using var verification = CreateDb();
        Assert.True(PasswordHasher.Verify("OldPassword123!",
            (await verification.Users.SingleAsync(user => user.IDUser == locked.IDUser)).MatKhau!));
    }

    private Task<VerifyResetCodeResult> Verify(User user, string code) => WithService(service =>
        service.VerifyResetCodeAsync(new() { UsernameOrEmail = user.Username, Code = code }, default));
    private Task<ResetPasswordResult> Reset(User user, string code) => WithService(service =>
        service.ResetPasswordAsync(new() { UsernameOrEmail = user.Username, Code = code, Password = "NewPassword123!" }, default));

    private async Task<T> WithService<T>(Func<AuthService, Task<T>> action)
    {
        await using var db = CreateDb();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = connectionString
        }).Build();
        var sessions = new AuthSessionService(
            db,
            new EfApplicationDbTransactionFactory(db),
            new AuthSessionMutations(db),
            new AuthSessionOptions { LifetimeMinutes = 60 },
            TimeProvider.System);
        return await action(new AuthService(db, emailSender, null!,
            new AuthQueries(new SqlConnectionFactory(config)), new PasswordResetTransactions(db), sessions));
    }

    private AtomosDbContext CreateDb() => new(new DbContextOptionsBuilder<AtomosDbContext>().UseSqlServer(connectionString).Options);

    private static AuthSessionService SessionService(AtomosDbContext db) => new(
        db,
        new EfApplicationDbTransactionFactory(db),
        new AuthSessionMutations(db),
        new AuthSessionOptions { LifetimeMinutes = 60 },
        TimeProvider.System);

    private async Task<User> CreateUserAsync()
    {
        await using var db = CreateDb();
        var marker = "otp" + Guid.NewGuid().ToString("N")[..16];
        var user = new User
        {
            IDRole = await db.Roles.Where(role => role.TenRole == "Student").Select(role => role.IDRole).SingleAsync(),
            Username = marker, Email = marker + "@example.test", MatKhau = PasswordHasher.Hash("OldPassword123!"),
            NgayTao = DateTime.Now, NgayCapNhat = DateTime.Now
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        userIds.Add(user.IDUser);
        return user;
    }

    private async Task<PasswordResetToken> CreateTokenAsync(int userId, bool expired = false)
    {
        await using var db = CreateDb();
        string code;
        do { code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6"); }
        while (await db.PasswordResetTokens.AnyAsync(token => token.Token == code));
        var token = new PasswordResetToken
        {
            IDUser = userId, Token = code, NgayTao = DateTime.Now,
            HetHan = DateTime.Now.AddMinutes(expired ? -1 : 30)
        };
        db.PasswordResetTokens.Add(token);
        await db.SaveChangesAsync();
        return token;
    }

    private sealed class NoMail : IEmailSender
    {
        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
