using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnterpriseHub.Infrastructure.Persistence.Mssql;

/// <summary>Used only by `dotnet ef migrations add` — the connection string here is never used at
/// runtime (the API composes its own via DI), just enough for EF's design-time tooling to build a model.</summary>
public sealed class MssqlDbContextFactory : IDesignTimeDbContextFactory<MssqlDbContext>
{
    public MssqlDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MssqlDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=EnterpriseHub;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        return new MssqlDbContext(options);
    }
}
