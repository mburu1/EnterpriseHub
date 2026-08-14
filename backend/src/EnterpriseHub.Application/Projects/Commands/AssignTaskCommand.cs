namespace EnterpriseHub.Application.Projects.Commands;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Projects.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Identity;
using EnterpriseHub.Domain.Projects;

public sealed record AssignTaskCommand(Guid TaskId, Guid AssigneeId) : ICommand<ProjectTaskDto>;

public sealed class AssignTaskCommandHandler(
    ITaskRepository taskRepository,
    IUserRepository userRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssignTaskCommand, ProjectTaskDto>
{
    public async Task<ProjectTaskDto> Handle(AssignTaskCommand command, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");

        var task = await taskRepository.GetByIdAsync(command.TaskId, tenantId, ct)
            ?? throw new DomainException("Task not found.");

        var assignee = await userRepository.GetByIdAsync(command.AssigneeId, ct);
        if (assignee is null || assignee.TenantId != tenantId)
            throw new DomainException("Assignee must be a member of the same organization.");

        task.AssignTo(command.AssigneeId);
        await unitOfWork.SaveChangesAsync(ct);

        return task.ToDto();
    }
}
