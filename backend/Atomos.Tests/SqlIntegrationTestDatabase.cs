using Microsoft.Data.SqlClient;

namespace Atomos.Tests;

internal static class SqlIntegrationTestDatabase
{
    private static readonly string[] AllowedPrefixes =
        ["Atomos_CI_", "Atomos_Test_", "Atomos_OtpTest_", "Atomos_AuthTest_"];

    public static string RequireConnectionString()
    {
        var configured = Environment.GetEnvironmentVariable("ATOMOS_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(configured))
        {
            throw new InvalidOperationException(
                "SQL integration BLOCKED: set ATOMOS_TEST_CONNECTION to a disposable Atomos_CI_* database.");
        }

        var database = new SqlConnectionStringBuilder(configured).InitialCatalog;
        if (!AllowedPrefixes.Any(prefix => database.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("SQL integration BLOCKED: refusing a non-disposable database.");
        }

        return configured;
    }
}
