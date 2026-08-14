namespace EnterpriseHub.Application.Projects.Commands;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Projects.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Projects;

public sealed record AddMilestoneCommand(Guid ProjectId, string Name, DateOnly? DueDate) : ICommand<MilestoneDto>;

public sealed class AddMilestoneCommandHandler(
    IProjectRepository projectRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddMilestoneCommand, MilestoneDto>
{
    public async Task<MilestoneDto> Handle(AddMilestoneCommand command, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");

        var project = await projectRepository.GetByIdAsync(command.ProjectId, tenantId, ct)
            ?? throw new DomainException("Project not found.");

        var milestone = project.AddMilestone(command.Name, command.DueDate);
        await unitOfWork.SaveChangesAsync(ct);

        return milestone.ToDto();
    }
}
