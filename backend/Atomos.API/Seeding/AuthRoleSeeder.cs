using Atomos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atomos.API.Seeding;

public static class AuthRoleSeeder
{
    public static async Task EnsureAuthRolesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AtomosDbContext>();
        await EnsureAuthRolesAsync(dbContext);
    }

    public static async Task EnsureAuthRolesAsync(AtomosDbContext dbContext)
    {
        await dbContext.Database.ExecuteSqlRawAsync("""
            MERGE dbo.[Role] WITH (HOLDLOCK) AS target
            USING (VALUES (N'Admin'), (N'Student')) AS source(TenRole)
                ON target.TenRole = source.TenRole
            WHEN NOT MATCHED THEN
                INSERT (TenRole) VALUES (source.TenRole);
            """);
    }
}
