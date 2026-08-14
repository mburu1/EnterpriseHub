using EnterpriseHub.Domain.Common;

namespace EnterpriseHub.Domain.Identity.Events;

public sealed record UserRegisteredEvent(Guid UserId, Guid TenantId, string Email) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
