using E_Commerce.Common.Domain.ValueObjects;

namespace E_Commerce.Common.Persistence.Services;

public class TenantService : ITenantService
{
    private TenantId? _currentTenant;

    public TenantId? GetCurrentTenant() => _currentTenant;

    public void SetTenant(TenantId tenantId) => _currentTenant = tenantId;
}