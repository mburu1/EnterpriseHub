// src/EnterpriseHub.Infrastructure/Persistence/Mssql/MssqlDbContextFactory.cs

using EnterpriseHub.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnterpriseHub.Infrastructure.Persistence.Mssql;

/// <summary>
/// Design-time factory for EF Core tooling (migrations, database update).
/// Connection string is resolved from appsettings.json at runtime of the tool —
/// never hardcoded, never used by the API host.
/// </summary>
public sealed class MssqlDbContextFactory : IDesignTimeDbContextFactory<MssqlDbContext>
{
    public MssqlDbContext CreateDbContext(string[] args)
    {
        var connectionString = DesignTimeConfiguration
            .Build()
            .RequireConnectionString("Mssql");

        var options = new DbContextOptionsBuilder<MssqlDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new MssqlDbContext(options);
    }
}
