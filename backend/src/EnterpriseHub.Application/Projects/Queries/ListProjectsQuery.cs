namespace EnterpriseHub.Application.Projects.Queries;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Projects.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Projects;

public sealed record ListProjectsQuery : IQuery<IReadOnlyList<ProjectDto>>;

public sealed class ListProjectsQueryHandler(IProjectRepository projectRepository, ICurrentUserService currentUser)
    : IQueryHandler<ListProjectsQuery, IReadOnlyList<ProjectDto>>
{
    public async Task<IReadOnlyList<ProjectDto>> Handle(ListProjectsQuery query, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");
        var projects = await projectRepository.ListByTenantAsync(tenantId, ct);
        return projects.Select(p => p.ToDto()).ToList();
    }
}
