namespace EnterpriseHub.Application.Projects.Commands;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Projects.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Projects;

public sealed record ChangeTaskStatusCommand(Guid TaskId, string Status) : ICommand<ProjectTaskDto>;

public sealed class ChangeTaskStatusCommandHandler(
    ITaskRepository taskRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeTaskStatusCommand, ProjectTaskDto>
{
    public async Task<ProjectTaskDto> Handle(ChangeTaskStatusCommand command, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");

        if (!Enum.TryParse<ProjectTaskStatus>(command.Status, ignoreCase: true, out var status))
            throw new DomainException($"'{command.Status}' is not a valid task status.");

        var task = await taskRepository.GetByIdAsync(command.TaskId, tenantId, ct)
            ?? throw new DomainException("Task not found.");

        task.ChangeStatus(status);
        await unitOfWork.SaveChangesAsync(ct);

        return task.ToDto();
    }
}
