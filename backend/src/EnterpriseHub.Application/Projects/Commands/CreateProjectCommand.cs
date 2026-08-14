namespace EnterpriseHub.Application.Projects.Commands;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Projects.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Projects;

public sealed record CreateProjectCommand(string Name, string? Description) : ICommand<ProjectDto>;

public sealed class CreateProjectCommandHandler(
    IProjectRepository projectRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProjectCommand, ProjectDto>
{
    public async Task<ProjectDto> Handle(CreateProjectCommand command, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");

        var project = Project.Create(tenantId, command.Name, command.Description);
        await projectRepository.AddAsync(project, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return project.ToDto();
    }
}
