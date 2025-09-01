using E_Commerce.Common.Infrastructure.Messaging;
using Microsoft.Extensions.Hosting;

namespace E_Commerce.Common.Messaging.RabbitMQ;

public class RabbitMqInitializationService(IMessageBroker messageBroker) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (messageBroker is RabbitMqMessageBroker rabbitMqBroker)
        {
            await rabbitMqBroker.InitializeAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
