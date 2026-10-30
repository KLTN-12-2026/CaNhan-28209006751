namespace Atomos.Application.Auth;

public interface IPasswordResetTransactions
{
    Task<IPasswordResetTransaction> BeginAsync(int userId, CancellationToken cancellationToken);
}

public interface IPasswordResetTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
