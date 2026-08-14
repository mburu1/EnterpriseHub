// src/EnterpriseHub.Infrastructure/Persistence/MySql/MySqlDbContextFactory.cs

using EnterpriseHub.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnterpriseHub.Infrastructure.Persistence.MySql;

/// <summary>
/// Design-time factory for EF Core tooling (migrations, database update).
/// Connection string is resolved from appsettings.json — never hardcoded.
/// </summary>
public sealed class MySqlDbContextFactory : IDesignTimeDbContextFactory<MySqlDbContext>
{
    public MySqlDbContext CreateDbContext(string[] args)
    {
        var connectionString = DesignTimeConfiguration
            .Build()
            .RequireConnectionString("MySql");

        // Prefer AutoDetect when the server is reachable at design time.
        // Fallback to explicit version when running offline / CI.
        var serverVersion = ServerVersion.AutoDetect(connectionString);
        // Explicit alternative (no DB round-trip):
        // var serverVersion = new MySqlServerVersion(new Version(8, 4, 0));

        var options = new DbContextOptionsBuilder<MySqlDbContext>()
            .UseMySql(connectionString, serverVersion)
            .Options;

        return new MySqlDbContext(options);
    }
}
