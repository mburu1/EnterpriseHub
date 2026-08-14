using EnterpriseHub.Domain.Common;

namespace EnterpriseHub.Domain.Projects.Events;

public sealed record TaskAssignedEvent(Guid TaskId, Guid ProjectId, Guid TenantId, Guid AssigneeId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
