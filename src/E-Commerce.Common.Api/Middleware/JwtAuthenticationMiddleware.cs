using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using E_Commerce.Common.Infrastructure.MultiTenancy;

namespace E_Commerce.Common.Api.Middleware;

public class JwtAuthenticationMiddleware(RequestDelegate next, ILogger<JwtAuthenticationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        // Extract tenant from JWT token or header
        var tenantId = ExtractTenantId(context);
        if (!string.IsNullOrEmpty(tenantId))
        {
            tenantService.SetTenant(Domain.ValueObjects.TenantId.Create(tenantId));
            logger.LogDebug("Tenant {TenantId} set for request {RequestPath}", tenantId, context.Request.Path);
        }

        await next(context);
    }

    private string? ExtractTenantId(HttpContext context)
    {
        // Try to get from JWT claims first
        var tenantClaim = context.User?.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantClaim))
            return tenantClaim;

        // Fallback to header
        return context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
    }
}
