namespace EnterpriseHub.Application.Projects.Commands;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Projects.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Projects;

public sealed record ChangeProjectStatusCommand(Guid ProjectId, string Status) : ICommand<ProjectDto>;

public sealed class ChangeProjectStatusCommandHandler(
    IProjectRepository projectRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeProjectStatusCommand, ProjectDto>
{
    public async Task<ProjectDto> Handle(ChangeProjectStatusCommand command, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");

        if (!Enum.TryParse<ProjectStatus>(command.Status, ignoreCase: true, out var status))
            throw new DomainException($"'{command.Status}' is not a valid project status.");

        var project = await projectRepository.GetByIdAsync(command.ProjectId, tenantId, ct)
            ?? throw new DomainException("Project not found.");

        project.ChangeStatus(status);
        await unitOfWork.SaveChangesAsync(ct);

        return project.ToDto();
    }
}
