using E_Commerce.Common.Domain.Primitives;

namespace E_Commerce.Common.Application.Services;

public interface IDomainEventPublisher
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent;
}