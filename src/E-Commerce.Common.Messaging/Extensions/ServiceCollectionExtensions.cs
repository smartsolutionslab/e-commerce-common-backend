using E_Commerce.Common.Messaging.Abstractions;
using E_Commerce.Common.Messaging.Configuration;
using E_Commerce.Common.Messaging.HealthChecks;
using E_Commerce.Common.Messaging.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Common.Messaging.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MessageBrokerConfig>(configuration.GetSection(MessageBrokerConfig.SectionName));
        services.AddSingleton<IMessageBroker, RabbitMqMessageBroker>();
        
        // Health Checks
        services.AddHealthChecks()
            .AddCheck<MessageBrokerHealthCheck>("message-broker");
        
        return services;
    }
}
