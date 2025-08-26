using E_Commerce.Common.Domain.Primitives;
using E_Commerce.Common.Messaging.Abstractions;
using E_Commerce.Common.Messaging.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace E_Commerce.Common.Messaging.RabbitMQ;

public class RabbitMqMessageBroker : IMessageBroker, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqMessageBroker> _logger;
    private readonly MessageBrokerConfig _config;

    public RabbitMqMessageBroker(IOptions<MessageBrokerConfig> config, ILogger<RabbitMqMessageBroker> logger)
    {
        _config = config.Value;
        _logger = logger;
        
        var factory = new ConnectionFactory()
        {
            HostName = _config.Host,
            Port = _config.Port,
            UserName = _config.Username,
            Password = _config.Password,
            VirtualHost = _config.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchanges
        _channel.ExchangeDeclare("domain.events", ExchangeType.Topic, true);
        _channel.ExchangeDeclare("integration.events", ExchangeType.Topic, true);
    }

    public async Task PublishAsync<T>(T message, string exchange, string routingKey, CancellationToken cancellationToken = default) where T : class
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = Guid.NewGuid().ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        _channel.BasicPublish(exchange, routingKey, properties, body);
        
        _logger.LogInformation("Published message {MessageType} to {Exchange}/{RoutingKey}", 
            typeof(T).Name, exchange, routingKey);

        await Task.CompletedTask;
    }

    public async Task PublishDomainEventAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : class, IDomainEvent
    {
        var routingKey = $"domain.event.{typeof(T).Name.ToLowerInvariant()}";
        await PublishAsync(domainEvent, "domain.events", routingKey, cancellationToken);
    }

    public async Task SubscribeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class
    {
        _channel.QueueDeclare(queueName, true, false, false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var message = JsonSerializer.Deserialize<T>(json);

                if (message != null)
                {
                    await handler(message);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue {QueueName}", queueName);
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel.BasicConsume(queueName, false, consumer);
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
