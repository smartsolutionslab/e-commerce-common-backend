using E_Commerce.Common.Domain.Primitives;

namespace E_Commerce.Common.Infrastructure.Messaging;

public interface IMessageBroker
{
    Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default) where T : class;
    
    Task PublishDomainEventAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : class, IDomainEvent;

    Task SubscribeAsync<T>(string queueName, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default) where T : class;
}
