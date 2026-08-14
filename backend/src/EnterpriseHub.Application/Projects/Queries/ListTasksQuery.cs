namespace EnterpriseHub.Application.Projects.Queries;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Projects.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Projects;

public sealed record ListTasksQuery(Guid ProjectId) : IQuery<IReadOnlyList<ProjectTaskDto>>;

public sealed class ListTasksQueryHandler(ITaskRepository taskRepository, ICurrentUserService currentUser)
    : IQueryHandler<ListTasksQuery, IReadOnlyList<ProjectTaskDto>>
{
    public async Task<IReadOnlyList<ProjectTaskDto>> Handle(ListTasksQuery query, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");
        var tasks = await taskRepository.ListByProjectAsync(query.ProjectId, tenantId, ct);
        return tasks.Select(t => t.ToDto()).ToList();
    }
}
