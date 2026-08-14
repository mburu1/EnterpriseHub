using System.Text.Json;
using Confluent.Kafka;
using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace EnterpriseHub.Infrastructure.Messaging.Kafka;

/// <summary>High-throughput audit event streaming: every domain event is projected onto the audit topic
/// independently of the operational RabbitMQ bus, for later consumption into the PostgreSQL audit log.</summary>
public sealed class KafkaAuditEventPublisher : IAuditEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public KafkaAuditEventPublisher(IOptions<KafkaOptions> options)
    {
        _topic = options.Value.AuditTopic;
        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            Acks = Acks.All,
            // Audit delivery is best-effort (see UnitOfWork, which already swallows failures here) —
            // these bound how long a request can be held up when the broker is unreachable, rather
            // than inheriting librdkafka's multi-minute defaults.
            MessageTimeoutMs = 5000,
            SocketConnectionSetupTimeoutMs = 5000
        }).Build();
    }

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var eventType = domainEvent.GetType().Name;
        var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());

        await _producer.ProduceAsync(_topic, new Message<string, string>
        {
            Key = eventType,
            Value = payload
        }, ct);
    }

    public void Dispose() => _producer.Dispose();
}
