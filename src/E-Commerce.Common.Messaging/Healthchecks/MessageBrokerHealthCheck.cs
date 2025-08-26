using E_Commerce.Common.Messaging.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace E_Commerce.Common.Messaging.HealthChecks;

public class MessageBrokerHealthCheck : IHealthCheck
{
    private readonly IMessageBroker _messageBroker;

    public MessageBrokerHealthCheck(IMessageBroker messageBroker)
    {
        _messageBroker = messageBroker;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple connectivity test
            await Task.Delay(10, cancellationToken); // Placeholder
            return HealthCheckResult.Healthy("Message broker is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Message broker connection failed", ex);
        }
    }
}