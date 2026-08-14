using EnterpriseHub.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseHub.Infrastructure.Persistence.Mssql.Repositories;

public sealed class ProjectRepository(MssqlDbContext dbContext) : IProjectRepository
{
    public Task<Project?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default) =>
        dbContext.Projects
            .Include(p => p.Milestones)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, ct);

    public async Task<IReadOnlyList<Project>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await dbContext.Projects.Where(p => p.TenantId == tenantId).ToListAsync(ct);

    public Task AddAsync(Project project, CancellationToken ct = default)
    {
        dbContext.Projects.Add(project);
        return Task.CompletedTask;
    }

    public void Update(Project project) => dbContext.Projects.Update(project);
}
