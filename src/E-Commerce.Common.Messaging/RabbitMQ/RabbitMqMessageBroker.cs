using System.Text;
using System.Text.Json;
using E_Commerce.Common.Domain.Primitives;
using E_Commerce.Common.Infrastructure.Messaging;
using E_Commerce.Common.Messaging.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace E_Commerce.Common.Messaging.RabbitMQ;

public class RabbitMqMessageBroker(IOptions<MessageBrokerConfig> options, ILogger<RabbitMqMessageBroker> logger)
    : IMessageBroker, IAsyncDisposable
{
    private readonly MessageBrokerConfig _options = options.Value;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public async Task InitializeAsync()
    {
        await _connectionLock.WaitAsync();
        try
        {
            if (_connection is not null)
                return;

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Host,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Exchange deklarieren
            await _channel.ExchangeDeclareAsync(
                exchange: _options.Exchange,
                type: ExchangeType.Topic,
                durable: true);

            logger.LogInformation("RabbitMQ connection established to {HostName}:{Port}", 
                _options.Host, _options.Port);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default) 
        where T : class
    {
        await EnsureInitializedAsync();

        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            ContentType = "application/json",
            Type = typeof(T).Name
        };

        await _channel!.BasicPublishAsync(
            exchange: _options.Exchange,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        logger.LogDebug("Published message {MessageType} with routing key {RoutingKey}", 
            typeof(T).Name, routingKey);
    }

    public async Task PublishDomainEventAsync<T>(T domainEvent, CancellationToken cancellationToken = default) 
        where T : class, IDomainEvent
    {
        await EnsureInitializedAsync();

        var eventName = typeof(T).Name;
        var routingKey = $"events.{eventName.ToLowerInvariant()}";

        var json = JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            ContentType = "application/json",
            Type = eventName,
            Headers = new Dictionary<string, object>
            {
                ["EventType"] = eventName,
                ["OccurredAt"] = domainEvent.OccurredOn.ToString("O")
            }!
        };

        await _channel!.BasicPublishAsync(
            exchange: _options.Exchange,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        logger.LogDebug("Published domain event {EventType} with routing key {RoutingKey}", 
            eventName, routingKey);
    }

    public async Task SubscribeAsync<T>(string queueName, Func<T, CancellationToken, Task> handler, 
        CancellationToken cancellationToken = default) where T : class
    {
        await EnsureInitializedAsync();

        // Queue deklarieren
        await _channel!.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        // Queue an Exchange binden
        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: _options.Exchange,
            routingKey: queueName,
            arguments: null,
            cancellationToken: cancellationToken);

        // QoS für bessere Lastverteilung
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                if (message is not null)
                {
                    await handler(message, cancellationToken);
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                }
                else
                {
                    logger.LogWarning("Failed to deserialize message to {MessageType}", typeof(T).Name);
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing message {MessageType} from queue {QueueName}", 
                    typeof(T).Name, queueName);
                
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: queueName, 
            autoAck: false, 
            consumer: consumer,
            cancellationToken: cancellationToken);

        logger.LogInformation("Started consuming messages from queue {QueueName}", queueName);
    }

    private async Task EnsureInitializedAsync()
    {
        if (_connection is null || _channel is null)
        {
            await InitializeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_channel is not null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disposing RabbitMQ connection");
        }
        finally
        {
            _connectionLock.Dispose();
        }
    }
}
