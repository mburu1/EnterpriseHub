using EnterpriseHub.Domain.Common;

namespace EnterpriseHub.Application.Common.Interfaces;

/// <summary>Publishes domain events onto the internal event bus (RabbitMQ) for cross-module reactions, e.g. task assigned -> notify member.</summary>
public interface IEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}

/// <summary>Streams domain events to the audit trail (Kafka) independently of the operational event bus.</summary>
public interface IAuditEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default);
}
