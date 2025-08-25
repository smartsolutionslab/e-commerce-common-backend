using E_Commerce.Common.Domain.ValueObjects;

namespace E_Commerce.Common.Infrastructure.Services;

public interface ITenantService
{
    TenantId? GetCurrentTenant();
    void SetTenant(TenantId tenantId);
}

public class TenantService : ITenantService
{
    private TenantId? _currentTenant;

    public TenantId? GetCurrentTenant() => _currentTenant;

    public void SetTenant(TenantId tenantId) => _currentTenant = tenantId;
}
