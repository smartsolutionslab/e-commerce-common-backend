using E_Commerce.Common.Infrastructure.HealthChecks;
using E_Commerce.Common.Infrastructure.Messaging;
using E_Commerce.Common.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Common.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Tenant Service
        services.AddScoped<ITenantService, TenantService>();
        
        // Message Broker
        services.AddSingleton<IMessageBroker, RabbitMqMessageBroker>();
        
        // Health Checks
        services.AddHealthChecks()
            .AddCheck<MessageBrokerHealthCheck>("message-broker");

        // Register event handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
            
        return services;
    }

    public static IServiceCollection AddDatabaseHealthCheck<T>(this IServiceCollection services) where T : DbContext
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck<T>>($"database-{typeof(T).Name.ToLower()}");
            
        return services;
    }
}
