using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnterpriseHub.Infrastructure.Persistence.Postgres;

/// <summary>Used only by `dotnet ef migrations add` — see MssqlDbContextFactory for rationale.</summary>
public sealed class PostgresDbContextFactory : IDesignTimeDbContextFactory<PostgresDbContext>
{
    public PostgresDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PostgresDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=enterprisehub_analytics")
            .Options;

        return new PostgresDbContext(options);
    }
}
