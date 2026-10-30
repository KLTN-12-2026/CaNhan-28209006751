using Atomos.Application.Common;
using Microsoft.Data.SqlClient;

namespace Atomos.Infrastructure.Persistence;

public sealed class DapperTransactionRunner(
    ISqlConnectionFactory connectionFactory,
    DapperTransactionContext transactionContext) : ITransactionRunner
{
    public Task RunAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default) =>
        RunAsync<object?>(async innerCancellationToken =>
        {
            await operation(innerCancellationToken);
            return null;
        }, cancellationToken);

    public async Task<T> RunAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (transactionContext.Current is not null)
        {
            return await operation(cancellationToken);
        }

        await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        var previous = transactionContext.Current;
        transactionContext.Current = new DapperTransactionScope(connection, transaction);

        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            transactionContext.Current = previous;
        }
    }
}
