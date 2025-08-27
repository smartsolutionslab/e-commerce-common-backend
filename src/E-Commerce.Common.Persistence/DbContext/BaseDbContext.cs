using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using E_Commerce.Common.Domain.Primitives;
using E_Commerce.Common.Application.Services;
using E_Commerce.Common.Messaging.Abstractions;
using E_Commerce.Common.Persistence.Services;

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
            .Entries()
            .Where(e => e.Entity is IEntity)
            .Select(e => e.Entity)
            .Cast<IEntity>()
            .SelectMany(entity => entity.GetDomainEvents())
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
        {
            await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
        }

        ChangeTracker
            .Entries()
            .Where(e => e.Entity is IEntity)
            .Select(e => e.Entity)
            .Cast<IEntity>()
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