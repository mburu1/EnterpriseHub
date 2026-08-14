using EnterpriseHub.Domain.Common;

namespace EnterpriseHub.Domain.Tenants.Events;

public sealed record TenantCreatedEvent(Guid TenantId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
