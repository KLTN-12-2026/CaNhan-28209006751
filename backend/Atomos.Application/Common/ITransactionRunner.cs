namespace Atomos.Application.Common;

public interface ITransactionRunner
{
    Task RunAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
    Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);
}
