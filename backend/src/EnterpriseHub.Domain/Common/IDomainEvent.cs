namespace EnterpriseHub.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
