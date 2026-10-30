using System.Security.Claims;
using Atomos.API.Controllers;
using Atomos.API.Security;
using Atomos.Application.Auth;
using Atomos.Domain.Users;
using Atomos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Atomos.Tests;

public sealed class AuthLifecycleIntegrationTests
{
    private static string ConnectionString => SqlIntegrationTestDatabase.RequireConnectionString();

    [Fact]
    public async Task ProfilePasswordChangeRevokesAllOwnSessionsAndLeavesOtherUserActive()
    {
        List<int> userIds = [];
        try
        {
            await using var db = CreateDb();
            var user = await CreateUser(db);
            var other = await CreateUser(db);
            userIds = [user.IDUser, other.IDUser];
            var sessions = SessionService(db);
            var passwordSession = await sessions.CreateSessionAsync(user.IDUser, AuthSessionMethod.Password);
            var googleSession = await sessions.CreateSessionAsync(user.IDUser, AuthSessionMethod.Google);
            var unrelated = await sessions.CreateSessionAsync(other.IDUser, AuthSessionMethod.Password);
            var controller = new ProfileController(
                db,
                null!,
                sessions,
                new EfApplicationDbTransactionFactory(db),
                null!)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(
                            [new Claim(ClaimTypes.NameIdentifier, user.IDUser.ToString())],
                            AtomosAuthenticationDefaults.CookieScheme))
                    }
                }
            };

            Assert.IsType<OkObjectResult>(await controller.ChangePassword(
                new ChangePasswordRequest("OldPassword123!", "NewPassword123!"),
                default));

            db.ChangeTracker.Clear();
            var revoked = await db.AuthSessions.AsNoTracking()
                .Where(session => session.IDAuthSession == passwordSession.SessionId
                    || session.IDAuthSession == googleSession.SessionId)
                .ToListAsync();
            Assert.All(revoked, session =>
            {
                Assert.NotNull(session.RevokedAt);
                Assert.Equal(AuthSessionRevokeReasons.PasswordChanged, session.RevokeReason);
            });
            Assert.Null(await sessions.GetValidSessionAsync(passwordSession.Secret));
            Assert.Null(await sessions.GetValidSessionAsync(googleSession.Secret));
            Assert.NotNull(await sessions.GetValidSessionAsync(unrelated.Secret));
        }
        finally
        {
            if (userIds.Count > 0)
            {
                await using var cleanup = CreateDb();
                await cleanup.Users.Where(user => userIds.Contains(user.IDUser)).ExecuteDeleteAsync();
            }
        }
    }

    private static AtomosDbContext CreateDb() => new(
        new DbContextOptionsBuilder<AtomosDbContext>()
            .UseSqlServer(ConnectionString)
            .Options);

    private static AuthSessionService SessionService(AtomosDbContext db) => new(
        db,
        new EfApplicationDbTransactionFactory(db),
        new AuthSessionMutations(db),
        new AuthSessionOptions { LifetimeMinutes = 60 },
        TimeProvider.System);

    private static async Task<User> CreateUser(AtomosDbContext db)
    {
        var roleId = await db.Roles.Where(role => role.TenRole == "Student")
            .Select(role => role.IDRole)
            .SingleAsync();
        var marker = Guid.NewGuid().ToString("N")[..12];
        var user = new User
        {
            IDRole = roleId,
            Username = $"lifecycle_{marker}",
            Email = $"lifecycle_{marker}@example.test",
            MatKhau = PasswordHasher.Hash("OldPassword123!"),
            NgayTao = DateTime.UtcNow,
            NgayCapNhat = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }
}
