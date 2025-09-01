using E_Commerce.Common.Domain.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Common.Application.Services;

public class DomainEventPublisher(IServiceProvider serviceProvider) : IDomainEventPublisher
{
    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken cancellationToken = default) where TEvent : IDomainEvent
    {
        var handlers = serviceProvider.GetServices<IDomainEventHandler<TEvent>>();
        
        var tasks = handlers.Select(handler => handler.HandleAsync(domainEvent, cancellationToken));
        await Task.WhenAll(tasks);
    }
}
