using EnterpriseHub.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseHub.Infrastructure.Persistence.Mssql.Repositories;

public sealed class TenantRepository(MssqlDbContext dbContext) : ITenantRepository
{
    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.Tenants.Include(t => t.Invitations).FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == slug, ct);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default) =>
        dbContext.Tenants.AnyAsync(t => t.Slug == slug, ct);

    public Task AddAsync(Tenant tenant, CancellationToken ct = default)
    {
        dbContext.Tenants.Add(tenant);
        return Task.CompletedTask;
    }

    public void Update(Tenant tenant) => dbContext.Tenants.Update(tenant);

    public Task<TenantInvitation?> GetInvitationByIdAsync(Guid invitationId, CancellationToken ct = default) =>
        dbContext.TenantInvitations.FirstOrDefaultAsync(i => i.Id == invitationId, ct);

    public async Task<IReadOnlyList<TenantInvitation>> ListInvitationsAsync(Guid tenantId, CancellationToken ct = default) =>
        await dbContext.TenantInvitations.Where(i => i.TenantId == tenantId).ToListAsync(ct);
}
