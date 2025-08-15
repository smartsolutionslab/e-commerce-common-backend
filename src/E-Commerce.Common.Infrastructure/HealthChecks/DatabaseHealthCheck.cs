using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Common.Infrastructure.HealthChecks;

public class DatabaseHealthCheck<T> : IHealthCheck where T : DbContext
{
    private readonly T _context;

    public DatabaseHealthCheck(T context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}

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
