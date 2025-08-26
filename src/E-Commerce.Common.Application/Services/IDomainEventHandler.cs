using E_Commerce.Common.Domain.Primitives;

namespace E_Commerce.Common.Application.Services;

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
