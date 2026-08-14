// src/EnterpriseHub.Infrastructure/Persistence/Postgres/PostgresDbContextFactory.cs

using EnterpriseHub.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnterpriseHub.Infrastructure.Persistence.Postgres;

/// <summary>
/// Design-time factory for EF Core tooling (migrations, database update).
/// Connection string is resolved from appsettings.json — never hardcoded.
/// </summary>
public sealed class PostgresDbContextFactory : IDesignTimeDbContextFactory<PostgresDbContext>
{
    public PostgresDbContext CreateDbContext(string[] args)
    {
        var connectionString = DesignTimeConfiguration
            .Build()
            .RequireConnectionString("Postgres");

        var options = new DbContextOptionsBuilder<PostgresDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new PostgresDbContext(options);
    }
}
