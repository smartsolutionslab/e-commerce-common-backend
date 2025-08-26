using E_Commerce.Common.Domain.Primitives;

namespace E_Commerce.Common.Messaging.Abstractions;

public interface IMessageBroker
{
    Task PublishAsync<T>(T message, string exchange, string routingKey, CancellationToken cancellationToken = default) where T : class;
    Task PublishDomainEventAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : class, IDomainEvent;
    Task SubscribeAsync<T>(string queueName, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class;
}
