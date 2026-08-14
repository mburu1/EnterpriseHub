namespace EnterpriseHub.Application.Projects.Queries;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Projects.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Projects;

public sealed record GetProjectQuery(Guid ProjectId) : IQuery<ProjectDto>;

public sealed class GetProjectQueryHandler(IProjectRepository projectRepository, ICurrentUserService currentUser)
    : IQueryHandler<GetProjectQuery, ProjectDto>
{
    public async Task<ProjectDto> Handle(GetProjectQuery query, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");
        var project = await projectRepository.GetByIdAsync(query.ProjectId, tenantId, ct)
            ?? throw new DomainException("Project not found.");

        return project.ToDto();
    }
}
