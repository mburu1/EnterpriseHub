using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseHub.Infrastructure.Persistence.Mssql;

/// <summary>
/// Commits the primary store's change tracker and, on success, publishes any domain events raised
/// by tracked aggregates to the internal event bus (RabbitMQ) and the audit stream (Kafka).
/// </summary>
public sealed class UnitOfWork(MssqlDbContext dbContext, IEventPublisher eventPublisher, IAuditEventPublisher auditPublisher)
    : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var aggregatesWithEvents = dbContext.ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        var result = await dbContext.SaveChangesAsync(ct);

        foreach (var aggregate in aggregatesWithEvents)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                await eventPublisher.PublishAsync(domainEvent, ct);
                await auditPublisher.PublishAsync(domainEvent, ct);
            }

            aggregate.ClearDomainEvents();
        }

        return result;
    }
}
