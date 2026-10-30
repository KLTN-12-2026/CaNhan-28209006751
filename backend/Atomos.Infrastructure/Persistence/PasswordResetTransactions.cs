using Atomos.Application.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Atomos.Infrastructure.Persistence;

public sealed class PasswordResetTransactions(AtomosDbContext db) : IPasswordResetTransactions
{
    public async Task<IPasswordResetTransaction> BeginAsync(int userId, CancellationToken cancellationToken)
    {
        var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // One database lock shared by resend, verify and reset, across API instances.
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT IDUser FROM dbo.NguoiDung WITH (UPDLOCK, HOLDLOCK) WHERE IDUser = {userId}",
                cancellationToken);
            foreach (var user in db.Users.Local.Where(user => user.IDUser == userId).ToArray())
                await db.Entry(user).ReloadAsync(cancellationToken);
            foreach (var token in db.PasswordResetTokens.Local.Where(token => token.IDUser == userId).ToArray())
                await db.Entry(token).ReloadAsync(cancellationToken);
            return new ResetTransaction(transaction);
        }
        catch
        {
            await transaction.DisposeAsync();
            throw;
        }
    }

    private sealed class ResetTransaction(IDbContextTransaction transaction) : IPasswordResetTransaction
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
