using Microsoft.Data.SqlClient;

namespace Atomos.Infrastructure.Persistence;

public interface ISqlConnectionFactory
{
    Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default);
}
