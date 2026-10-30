namespace Atomos.Application.Auth;

public interface IAuthQueries
{
    Task<AuthCurrentAccountReadModel?> FindCurrentAccountAsync(
        int userId,
        CancellationToken cancellationToken);

    Task<AuthResetUserReadModel?> FindUserForResetVerificationAsync(
        string normalizedUsernameOrEmail,
        CancellationToken cancellationToken);

    Task<ResetTokenReadModel?> FindValidResetTokenAsync(
        int userId,
        string code,
        DateTime now,
        CancellationToken cancellationToken);
}

public sealed record AuthCurrentAccountReadModel(int Id, string Role, bool IsLocked, bool IsDeleted);

public sealed record AuthResetUserReadModel(int Id);

public sealed record ResetTokenReadModel(int Id);
