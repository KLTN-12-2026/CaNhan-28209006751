using Atomos.Application.Auth;
using Atomos.Infrastructure.Persistence;
using Dapper;

namespace Atomos.Infrastructure.Queries;

public sealed class AuthQueries(ISqlConnectionFactory connectionFactory) : IAuthQueries
{
    public async Task<AuthCurrentAccountReadModel?> FindCurrentAccountAsync(
        int userId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                userRow.IDUser AS Id,
                roleRow.TenRole AS Role,
                userRow.DaKhoa AS IsLocked,
                userRow.DaXoa AS IsDeleted
            FROM dbo.NguoiDung AS userRow
            INNER JOIN dbo.[Role] AS roleRow ON roleRow.IDRole = userRow.IDRole
            WHERE userRow.IDUser = @UserId;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<AuthCurrentAccountReadModel>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<AuthResetUserReadModel?> FindUserForResetVerificationAsync(
        string normalizedUsernameOrEmail,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT IDUser AS Id
            FROM dbo.NguoiDung
            WHERE LOWER(Username) = @NormalizedUsernameOrEmail
               OR LOWER(Email) = @NormalizedUsernameOrEmail;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<AuthResetUserReadModel>(
            new CommandDefinition(
                sql,
                new { NormalizedUsernameOrEmail = normalizedUsernameOrEmail },
                cancellationToken: cancellationToken));
    }

    public async Task<ResetTokenReadModel?> FindValidResetTokenAsync(
        int userId,
        string code,
        DateTime now,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT IDReset AS Id
            FROM dbo.PasswordResetTokens
            WHERE IDUser = @UserId
              AND Token = @Code
              AND DaSuDung = CAST(0 AS bit)
              AND HetHan > @Now;
            """;

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<ResetTokenReadModel>(
            new CommandDefinition(
                sql,
                new { UserId = userId, Code = code, Now = now },
                cancellationToken: cancellationToken));
    }
}
