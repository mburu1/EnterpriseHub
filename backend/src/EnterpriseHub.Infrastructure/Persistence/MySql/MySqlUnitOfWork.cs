using EnterpriseHub.Application.Common.Interfaces;

namespace EnterpriseHub.Infrastructure.Persistence.MySql;

public sealed class MySqlUnitOfWork(MySqlDbContext dbContext) : IBillingUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => dbContext.SaveChangesAsync(ct);
}
