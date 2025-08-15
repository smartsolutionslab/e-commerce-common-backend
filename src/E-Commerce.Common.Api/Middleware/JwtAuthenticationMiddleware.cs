using E_Commerce.Common.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace E_Commerce.Common.Api.Middleware;

public class JwtAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtAuthenticationMiddleware> _logger;

    public JwtAuthenticationMiddleware(RequestDelegate next, ILogger<JwtAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        // Extract tenant from JWT token or header
        var tenantId = ExtractTenantId(context);
        if (!string.IsNullOrEmpty(tenantId))
        {
            tenantService.SetTenant(Domain.ValueObjects.TenantId.Create(tenantId));
            _logger.LogDebug("Tenant {TenantId} set for request {RequestPath}", tenantId, context.Request.Path);
        }

        await _next(context);
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
