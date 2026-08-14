using EnterpriseHub.Domain.Common;
using EnterpriseHub.Domain.Projects.Events;

namespace EnterpriseHub.Domain.Projects;

public sealed class ProjectTask : AggregateRoot<Guid>, ITenantScoped
{
    public Guid TenantId { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public ProjectTaskStatus Status { get; private set; } = ProjectTaskStatus.Todo;
    public TaskPriority Priority { get; private set; } = TaskPriority.Medium;
    public Guid? AssigneeId { get; private set; }
    public DateOnly? DueDate { get; private set; }

    private ProjectTask() { }

    public static ProjectTask Create(Guid tenantId, Guid projectId, string title, string? description, TaskPriority priority, DateOnly? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Task title is required.");

        return new ProjectTask
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = projectId,
            Title = title.Trim(),
            Description = description,
            Priority = priority,
            DueDate = dueDate
        };
    }

    public void AssignTo(Guid assigneeId)
    {
        AssigneeId = assigneeId;
        Touch();
        Raise(new TaskAssignedEvent(Id, ProjectId, TenantId, assigneeId));
    }

    public void ChangeStatus(ProjectTaskStatus newStatus)
    {
        if (Status == newStatus) return;
        var previous = Status;
        Status = newStatus;
        Touch();
        Raise(new TaskStatusChangedEvent(Id, ProjectId, TenantId, previous, newStatus));
    }
}
