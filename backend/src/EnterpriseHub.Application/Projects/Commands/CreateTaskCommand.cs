namespace EnterpriseHub.Application.Projects.Commands;

using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Application.Common.Messaging;
using EnterpriseHub.Application.Projects.Dtos;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Projects;

public sealed record CreateTaskCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    string Priority,
    DateOnly? DueDate) : ICommand<ProjectTaskDto>;

public sealed class CreateTaskCommandHandler(
    IProjectRepository projectRepository,
    ITaskRepository taskRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTaskCommand, ProjectTaskDto>
{
    public async Task<ProjectTaskDto> Handle(CreateTaskCommand command, CancellationToken ct)
    {
        var tenantId = currentUser.TenantId ?? throw new ForbiddenException("No tenant context on the current user.");

        if (!Enum.TryParse<TaskPriority>(command.Priority, ignoreCase: true, out var priority))
            throw new DomainException($"'{command.Priority}' is not a valid task priority.");

        _ = await projectRepository.GetByIdAsync(command.ProjectId, tenantId, ct)
            ?? throw new DomainException("Project not found.");

        var task = ProjectTask.Create(tenantId, command.ProjectId, command.Title, command.Description, priority, command.DueDate);
        await taskRepository.AddAsync(task, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return task.ToDto();
    }
}
