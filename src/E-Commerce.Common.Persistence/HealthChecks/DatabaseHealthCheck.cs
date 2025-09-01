using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace E_Commerce.Common.Persistence.HealthChecks;

public class DatabaseHealthCheck<T>(T context) : IHealthCheck
    where T : Microsoft.EntityFrameworkCore.DbContext
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context1, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}