using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Infrastructure.Cache;

namespace EnterpriseHub.Tests.E2E.Fakes;

/// <summary>Test doubles for the auth vertical slice: swaps out RabbitMQ/Kafka/SMTP/Redis so the E2E
/// suite only needs a real SQL Server (via Testcontainers) to exercise AuthController end to end.</summary>
internal sealed class NoOpEventPublisher : IEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default) => Task.CompletedTask;
}

internal sealed class NoOpAuditEventPublisher : IAuditEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default) => Task.CompletedTask;
}

internal sealed class NoOpEmailSender : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default) => Task.CompletedTask;
}

internal sealed class AllowAllRateLimiter : ITenantRateLimiter
{
    public Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window, CancellationToken ct = default) =>
        Task.FromResult(true);
}
