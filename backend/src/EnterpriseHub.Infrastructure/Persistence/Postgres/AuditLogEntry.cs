namespace EnterpriseHub.Infrastructure.Persistence.Postgres;

/// <summary>Denormalized projection of a domain event, written by the Kafka audit consumer for analytics/dashboards.</summary>
public sealed class AuditLogEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string EventType { get; init; } = null!;
    public Guid? TenantId { get; init; }
    public string PayloadJson { get; init; } = null!;
    public DateTimeOffset OccurredOn { get; init; }
}
