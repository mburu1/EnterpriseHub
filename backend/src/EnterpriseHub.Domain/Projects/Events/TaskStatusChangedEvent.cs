using EnterpriseHub.Domain.Common;

namespace EnterpriseHub.Domain.Projects.Events;

public sealed record TaskStatusChangedEvent(Guid TaskId, Guid ProjectId, Guid TenantId, ProjectTaskStatus PreviousStatus, ProjectTaskStatus NewStatus) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
