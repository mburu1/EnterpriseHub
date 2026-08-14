using System.Text;
using System.Text.Json;
using EnterpriseHub.Application.Common.Interfaces;
using EnterpriseHub.Domain.Common;
using EnterpriseHub.Infrastructure.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EnterpriseHub.Infrastructure.Messaging.RabbitMQ;

/// <summary>
/// Publishes domain events onto the internal event bus, e.g. TaskAssignedEvent -> notification consumer.
/// A connection is opened lazily and reused across publishes.
/// </summary>
public sealed class RabbitMqEventPublisher(IOptions<RabbitMqOptions> options) : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options = options.Value;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var channel = await GetChannelAsync(ct);
        var routingKey = domainEvent.GetType().Name;
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(domainEvent, domainEvent.GetType()));

        await channel.BasicPublishAsync(
            exchange: _options.Exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: new BasicProperties { ContentType = "application/json", Persistent = true },
            body: body,
            cancellationToken: ct);
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken ct)
    {
        if (_channel is not null) return _channel;

        await _lock.WaitAsync(ct);
        try
        {
            if (_channel is not null) return _channel;

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.User,
                Password = _options.Password
            };

            _connection = await factory.CreateConnectionAsync(ct);
            _channel = await _connection.CreateChannelAsync(cancellationToken: ct);
            await _channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Topic, durable: true, cancellationToken: ct);
            return _channel;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
        _lock.Dispose();
    }
}
