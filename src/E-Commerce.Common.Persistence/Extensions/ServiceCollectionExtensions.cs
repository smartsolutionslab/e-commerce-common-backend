using E_Commerce.Common.Application.Abstractions;
using E_Commerce.Common.Persistence.DbContext;
using E_Commerce.Common.Persistence.Services;
using E_Commerce.Common.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace E_Commerce.Common.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommonPersistence(this IServiceCollection services)
    {
        services.AddScoped<ITenantService, TenantService>();
        return services;
    }

    public static IServiceCollection AddUnitOfWork<TDbContext>(this IServiceCollection services)
        where TDbContext : BaseDbContext
    {
        services.AddScoped<IUnitOfWork, UnitOfWork<TDbContext>>();
        return services;
    }

    public static IServiceCollection AddSqlServerDatabase<TContext>(this IServiceCollection services, IConfiguration configuration, string connectionStringName = "DefaultConnection")
        where TContext : Microsoft.EntityFrameworkCore.DbContext
    {
        services.AddDbContext<TContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(connectionStringName)));

        return services;
    }

    public static IServiceCollection AddDatabaseHealthCheck(this IServiceCollection services, IConfiguration configuration, string connectionStringName = "DefaultConnection", string healthCheckName = "database")
    {
        services.AddHealthChecks()
            .AddSqlServer(
                configuration.GetConnectionString(connectionStringName) ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found."),
                name: healthCheckName);

        return services;
    }
}
