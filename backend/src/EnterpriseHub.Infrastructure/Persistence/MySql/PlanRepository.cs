using EnterpriseHub.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseHub.Infrastructure.Persistence.MySql;

public sealed class PlanRepository(MySqlDbContext dbContext) : IPlanRepository
{
    public Task<Plan?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.Plans.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Plan>> ListAsync(CancellationToken ct = default) =>
        await dbContext.Plans.ToListAsync(ct);
}
