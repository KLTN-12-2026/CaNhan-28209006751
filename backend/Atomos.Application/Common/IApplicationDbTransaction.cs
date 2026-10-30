namespace Atomos.Application.Common;

public interface IApplicationDbTransactionFactory
{
    Task<IApplicationDbTransaction> BeginAsync(CancellationToken cancellationToken);
}

public interface IApplicationDbTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken);
}
