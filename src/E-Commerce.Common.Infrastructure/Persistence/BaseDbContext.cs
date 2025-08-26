using E_Commerce.Common.Domain.Primitives;
using E_Commerce.Common.Infrastructure.Services;
using E_Commerce.Common.Infrastructure.Messaging;
using E_Commerce.Common.Application.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace E_Commerce.Common.Infrastructure.Persistence;

public abstract class BaseDbContext : DbContext
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply tenant filter to all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Entity<>).IsAssignableFromGeneric(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(CreateTenantFilter(entityType.ClrType));
            }
        }

        base.OnModelCreating(modelBuilder);
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

    private LambdaExpression CreateTenantFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var tenantProperty = Expression.Property(parameter, nameof(Entity<object>.TenantId));
        var tenantValue = Expression.Property(
            Expression.Constant(_tenantService), 
            nameof(ITenantService.TenantId));
        
        var equal = Expression.Equal(tenantProperty, tenantValue);
        return Expression.Lambda(equal, parameter);
    }
}

public static class TypeExtensions
{
    public static bool IsAssignableFromGeneric(this Type genericType, Type type)
    {
        return type.GetInterfaces().Any(i => 
            i.IsGenericType && i.GetGenericTypeDefinition() == genericType) ||
            (type.BaseType?.IsGenericType == true && 
             type.BaseType.GetGenericTypeDefinition() == genericType);
    }
}