using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseHub.Infrastructure.Persistence.Mssql;

/// <summary>
/// Commits the primary store's change tracker and, on success, publishes any domain events raised
/// by tracked aggregates to the internal event bus (RabbitMQ) and the audit stream (Kafka).
/// </summary>
public sealed class UnitOfWork(
    MssqlDbContext dbContext,
    IEventPublisher eventPublisher,
    IAuditEventPublisher auditPublisher,
    ILogger<UnitOfWork> logger)
    : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var aggregatesWithEvents = dbContext.ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        var result = await dbContext.SaveChangesAsync(ct);

        // Event delivery is at-least-once/best-effort (see ADR-002), not part of the transaction
        // above, which has already committed by this point. A broker outage must not surface as a
        // failed request for a write that actually succeeded — so publish failures are logged and
        // swallowed rather than propagated, matching the same fail-open posture as the rate limiter
        // (see TenantRateLimitingMiddleware / ADR-004).
        foreach (var aggregate in aggregatesWithEvents)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                try
                {
                    await eventPublisher.PublishAsync(domainEvent, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to publish {EventType} to the event bus.", domainEvent.GetType().Name);
                }

                try
                {
                    await auditPublisher.PublishAsync(domainEvent, ct);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to publish {EventType} to the audit stream.", domainEvent.GetType().Name);
                }
            }

            aggregate.ClearDomainEvents();
        }

        return result;
    }
}
