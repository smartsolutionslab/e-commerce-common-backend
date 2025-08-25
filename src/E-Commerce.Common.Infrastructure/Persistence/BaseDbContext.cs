using E_Commerce.Common.Domain.Primitives;
using E_Commerce.Common.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Common.Infrastructure.Persistence;

public abstract class BaseDbContext : DbContext
{
    private readonly ITenantService _tenantService;
    private readonly IPublisher _publisher;

    protected BaseDbContext(DbContextOptions options, ITenantService tenantService, IPublisher publisher)
        : base(options)
    {
        _tenantService = tenantService;
        _publisher = publisher;
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
            await _publisher.Publish(domainEvent, cancellationToken);
        }

        ChangeTracker
            .Entries<Entity<object>>()
            .Select(x => x.Entity)
            .ToList()
            .ForEach(entity => entity.ClearDomainEvents());

        return result;
    }
}
