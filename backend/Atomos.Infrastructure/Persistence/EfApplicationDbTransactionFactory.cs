using Atomos.Application.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace Atomos.Infrastructure.Persistence;

public sealed class EfApplicationDbTransactionFactory(AtomosDbContext db) : IApplicationDbTransactionFactory
{
    public async Task<IApplicationDbTransaction> BeginAsync(CancellationToken cancellationToken)
    {
        if (db.Database.CurrentTransaction is not null)
        {
            return BorrowedApplicationDbTransaction.Instance;
        }

        return new EfApplicationDbTransaction(await db.Database.BeginTransactionAsync(cancellationToken));
    }

    private sealed class BorrowedApplicationDbTransaction : IApplicationDbTransaction
    {
        public static readonly BorrowedApplicationDbTransaction Instance = new();

        public Task CommitAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class EfApplicationDbTransaction(IDbContextTransaction transaction) : IApplicationDbTransaction
    {
        private bool disposed;

        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            await transaction.CommitAsync(cancellationToken);
            await DisposeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (disposed) return;
            disposed = true;
            await transaction.DisposeAsync();
        }
    }
}
