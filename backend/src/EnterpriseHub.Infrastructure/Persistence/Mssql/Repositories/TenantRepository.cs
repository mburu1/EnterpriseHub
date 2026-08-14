using EnterpriseHub.Domain.Tenants;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseHub.Infrastructure.Persistence.Mssql.Repositories;

public sealed class TenantRepository(MssqlDbContext dbContext) : ITenantRepository
{
    public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);

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
}
