namespace EnterpriseHub.Domain.Tenants;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    void Update(Tenant tenant);

    Task<TenantInvitation?> GetInvitationByIdAsync(Guid invitationId, CancellationToken ct = default);
    Task<IReadOnlyList<TenantInvitation>> ListInvitationsAsync(Guid tenantId, CancellationToken ct = default);
}
