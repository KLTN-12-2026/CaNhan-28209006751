using Atomos.Application.Auth;
using Atomos.Application.Common;
using Atomos.Domain.Users;
using Atomos.Infrastructure.Persistence;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Atomos.Tests;

public sealed class AuthAccountLinkingTests
{
    private static string ConnectionString
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable("ATOMOS_TEST_CONNECTION");
            if (string.IsNullOrWhiteSpace(configured))
                throw new InvalidOperationException("SQL integration BLOCKED: set ATOMOS_TEST_CONNECTION to a disposable Atomos_CI_* or Atomos_AuthTest_* database.");
            var database = new SqlConnectionStringBuilder(configured).InitialCatalog;
            if (!database.StartsWith("Atomos_CI_", StringComparison.OrdinalIgnoreCase)
                && !database.StartsWith("Atomos_AuthTest_", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Security tests refuse to mutate a non-disposable database.");
            return configured;
        }
    }

    [Fact]
    public async Task UnverifiedPasswordAccountCannotBeSilentlyClaimedByGoogle()
    {
        await using var db = CreateDb();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await CreateUser(db, password: "AttackerPassword123!");
        var service = Service(db, GoogleUser(user.Email, "google-victim"));
        var result = await service.GoogleAuthAsync(new() { IdToken = "valid" }, default);

        Assert.False(result.Success);
        Assert.Equal("GOOGLE_EMAIL_LINK_REQUIRED", result.Code);
        db.ChangeTracker.Clear();
        var stored = await db.Users.SingleAsync(item => item.IDUser == user.IDUser);
        Assert.Null(stored.IDGoogle);
        Assert.True(PasswordHasher.Verify("AttackerPassword123!", stored.MatKhau!));
        Assert.True((await service.LoginAsync(new() { UsernameOrEmail = user.Email, Password = "AttackerPassword123!" }, default)).Success);
        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task ExistingGoogleSubjectStillLogsInAndDifferentSubjectCannotUseItsEmail()
    {
        await using var db = CreateDb();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await CreateUser(db, googleId: "google-owner");

        var valid = await Service(db, GoogleUser(user.Email, "google-owner")).GoogleAuthAsync(new() { IdToken = "valid" }, default);
        var rejected = await Service(db, GoogleUser(user.Email, "google-other")).GoogleAuthAsync(new() { IdToken = "valid" }, default);

        Assert.True(valid.Success);
        Assert.False(rejected.Success);
        Assert.Equal("GOOGLE_EMAIL_LINK_REQUIRED", rejected.Code);
        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task NewGoogleIdentityStillCompletesNormalRegistration()
    {
        await using var db = CreateDb();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var email = $"google_{Guid.NewGuid():N}@example.test";
        var service = Service(db, GoogleUser(email, $"google-{Guid.NewGuid():N}"));

        var prompt = await service.GoogleAuthAsync(new() { IdToken = "valid" }, default);
        var created = await service.GoogleAuthAsync(new() { IdToken = "valid", Username = $"g{Guid.NewGuid():N}"[..20] }, default);

        Assert.True(prompt.Success);
        Assert.True(prompt.Data!.RequiresUsername);
        Assert.True(created.Success);
        Assert.False(created.Data!.RequiresUsername);
        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task RepeatedAndConcurrentEmailCollisionsNeverAttachGoogleIdentity()
    {
        await using var setup = CreateDb();
        var user = await CreateUser(setup, password: "AttackerPassword123!");
        try
        {
            var attempts = Enumerable.Range(0, 4).Select(async index =>
            {
                await using var db = CreateDb();
                return await Service(db, GoogleUser(user.Email, $"google-{index}")).GoogleAuthAsync(new() { IdToken = "valid" }, default);
            });
            var results = await Task.WhenAll(attempts);
            Assert.All(results, result => Assert.Equal("GOOGLE_EMAIL_LINK_REQUIRED", result.Code));
            setup.ChangeTracker.Clear();
            Assert.Null((await setup.Users.SingleAsync(item => item.IDUser == user.IDUser)).IDGoogle);
        }
        finally
        {
            setup.Users.Remove(await setup.Users.SingleAsync(item => item.IDUser == user.IDUser));
            await setup.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task LoginUpgradesLegacyPbkdf2HashWithoutChangingCredentials()
    {
        await using var db = CreateDb();
        await using var transaction = await db.Database.BeginTransactionAsync();
        const string password = "LegacyPassword123!";
        var user = await CreateUser(db);
        user.MatKhau = LegacyHash(password);
        var original = user.MatKhau;
        await db.SaveChangesAsync();

        var result = await Service(db, GoogleUser(user.Email, "unused")).LoginAsync(
            new() { UsernameOrEmail = user.Username, Password = password }, default);

        Assert.True(result.Success);
        Assert.NotEqual(original, user.MatKhau);
        Assert.True(PasswordHasher.Verify(password, user.MatKhau!, out var needsRehash));
        Assert.False(needsRehash);
        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task LoginFailuresDoNotRevealAccountState()
    {
        await using var db = CreateDb();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = await CreateUser(db, password: "CorrectPassword123!");
        var service = Service(db, GoogleUser(user.Email, "unused"));
        var wrong = await service.LoginAsync(new() { UsernameOrEmail = user.Username, Password = "WrongPassword123!" }, default);
        var missing = await service.LoginAsync(new() { UsernameOrEmail = $"missing{Guid.NewGuid():N}"[..30], Password = "WrongPassword123!" }, default);
        user.DaKhoa = true;
        await db.SaveChangesAsync();
        var locked = await service.LoginAsync(new() { UsernameOrEmail = user.Username, Password = "CorrectPassword123!" }, default);
        user.DaKhoa = false;
        user.MatKhau = null;
        await db.SaveChangesAsync();
        var externalOnly = await service.LoginAsync(new() { UsernameOrEmail = user.Username, Password = "WrongPassword123!" }, default);

        Assert.Equal((wrong.Code, wrong.Message), (missing.Code, missing.Message));
        Assert.Equal((wrong.Code, wrong.Message), (locked.Code, locked.Message));
        Assert.Equal((wrong.Code, wrong.Message), (externalOnly.Code, externalOnly.Message));
        await transaction.RollbackAsync();
    }

    private static string LegacyHash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"PBKDF2-SHA256.100000.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    private static AuthService Service(AtomosDbContext db, GoogleUserInfo googleUser) =>
        new(db, null!, new Google(googleUser), null!, null!, null!);

    private static GoogleUserInfo GoogleUser(string email, string id) => new() { Email = email, GoogleId = id };

    private static AtomosDbContext CreateDb() => new(new DbContextOptionsBuilder<AtomosDbContext>()
        .UseSqlServer(ConnectionString).Options);

    private static async Task<User> CreateUser(AtomosDbContext db, string? password = null, string? googleId = null)
    {
        var marker = Guid.NewGuid().ToString("N");
        var user = new User
        {
            IDRole = await db.Roles.Where(role => role.TenRole == "Student").Select(role => role.IDRole).SingleAsync(),
            Username = $"auth_{marker}"[..30],
            Email = $"auth_{marker}@example.test",
            MatKhau = password is null ? null : PasswordHasher.Hash(password),
            IDGoogle = googleId,
            NgayTao = DateTime.Now,
            NgayCapNhat = DateTime.Now
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private sealed class Google(GoogleUserInfo user) : IGoogleTokenVerifier
    {
        public Task<GoogleUserInfo?> VerifyAsync(string idToken, CancellationToken cancellationToken) => Task.FromResult<GoogleUserInfo?>(user);
    }

}
