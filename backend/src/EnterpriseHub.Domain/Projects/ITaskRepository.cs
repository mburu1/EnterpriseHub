namespace EnterpriseHub.Domain.Projects;

public interface ITaskRepository
{
    Task<ProjectTask?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<ProjectTask>> ListByProjectAsync(Guid projectId, Guid tenantId, CancellationToken ct = default);
    Task AddAsync(ProjectTask task, CancellationToken ct = default);
    void Update(ProjectTask task);
}
