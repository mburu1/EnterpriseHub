using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EnterpriseHub.Infrastructure.Persistence.MySql;

/// <summary>Used only by `dotnet ef migrations add` — see MssqlDbContextFactory for rationale.</summary>
public sealed class MySqlDbContextFactory : IDesignTimeDbContextFactory<MySqlDbContext>
{
    public MySqlDbContext CreateDbContext(string[] args)
    {
        const string connectionString = "Server=localhost;Port=3306;Database=enterprisehub_billing;User=root;Password=root;";
        var options = new DbContextOptionsBuilder<MySqlDbContext>()
            .UseMySql(connectionString, ServerVersion.Create(new Version(8, 4, 0), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql))
            .Options;

        return new MySqlDbContext(options);
    }
}
