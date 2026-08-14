namespace EnterpriseHub.Domain.Projects;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<Project>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Project project, CancellationToken ct = default);
    void Update(Project project);
}
