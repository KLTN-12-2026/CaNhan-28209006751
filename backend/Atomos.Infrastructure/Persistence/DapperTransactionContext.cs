using System.Threading;
using Microsoft.Data.SqlClient;

namespace Atomos.Infrastructure.Persistence;

public sealed class DapperTransactionContext
{
    private readonly AsyncLocal<DapperTransactionScope?> current = new();

    public DapperTransactionScope? Current
    {
        get => current.Value;
        set => current.Value = value;
    }
}

public sealed record DapperTransactionScope(SqlConnection Connection, SqlTransaction Transaction);
