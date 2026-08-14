using EnterpriseHub.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseHub.Infrastructure.Persistence.MySql;

public sealed class SubscriptionRepository(MySqlDbContext dbContext) : ISubscriptionRepository
{
    public Task<Subscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default) =>
        dbContext.Subscriptions.FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

    public Task AddAsync(Subscription subscription, CancellationToken ct = default)
    {
        dbContext.Subscriptions.Add(subscription);
        return Task.CompletedTask;
    }

    public void Update(Subscription subscription) => dbContext.Subscriptions.Update(subscription);
}
