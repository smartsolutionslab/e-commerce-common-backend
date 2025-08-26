using E_Commerce.Common.Domain.Primitives;
using E_Commerce.Common.Application.Services;
using E_Commerce.Common.Messaging.Abstractions;
using E_Commerce.Common.Persistence.Services;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Common.Persistence.DbContext;

public abstract class BaseDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    private readonly ITenantService _tenantService;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly IMessageBroker _messageBroker;

    protected BaseDbContext(DbContextOptions options, ITenantService tenantService, IDomainEventPublisher domainEventPublisher, IMessageBroker messageBroker)
        : base(options)
    {
        _tenantService = tenantService;
        _domainEventPublisher = domainEventPublisher;
        _messageBroker = messageBroker;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<Entity<object>>()
            .Select(x => x.Entity)
            .SelectMany(x => x.GetDomainEvents())
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
        {
            await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
        }

        ChangeTracker
            .Entries<Entity<object>>()
            .Select(x => x.Entity)
            .ToList()
            .ForEach(entity => entity.ClearDomainEvents());

        return result;
    }
}
