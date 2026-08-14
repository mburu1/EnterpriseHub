namespace EnterpriseHub.Infrastructure.Persistence.Oracle;

/// <summary>A scheduled, materialized report (ADR-001: Oracle demonstrates multi-DB fluency for the reporting schema).</summary>
public sealed class ReportSnapshot
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public string ReportType { get; init; } = null!;
    public string DataJson { get; init; } = null!;
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;
}
