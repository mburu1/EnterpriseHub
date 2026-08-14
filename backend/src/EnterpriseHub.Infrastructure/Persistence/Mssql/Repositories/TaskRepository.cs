using EnterpriseHub.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseHub.Infrastructure.Persistence.Mssql.Repositories;

public sealed class TaskRepository(MssqlDbContext dbContext) : ITaskRepository
{
    public Task<ProjectTask?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default) =>
        dbContext.ProjectTasks.FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId, ct);

    public async Task<IReadOnlyList<ProjectTask>> ListByProjectAsync(Guid projectId, Guid tenantId, CancellationToken ct = default) =>
        await dbContext.ProjectTasks
            .Where(t => t.ProjectId == projectId && t.TenantId == tenantId)
            .ToListAsync(ct);

    public Task AddAsync(ProjectTask task, CancellationToken ct = default)
    {
        dbContext.ProjectTasks.Add(task);
        return Task.CompletedTask;
    }

    public void Update(ProjectTask task) => dbContext.ProjectTasks.Update(task);
}
