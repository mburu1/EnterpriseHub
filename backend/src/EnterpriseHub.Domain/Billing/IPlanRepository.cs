namespace EnterpriseHub.Domain.Billing;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Plan>> ListAsync(CancellationToken ct = default);
}
