namespace EnterpriseHub.Domain.Billing;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Subscription subscription, CancellationToken ct = default);
    void Update(Subscription subscription);
}
