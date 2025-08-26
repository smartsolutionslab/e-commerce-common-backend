using E_Commerce.Common.Domain.ValueObjects;

namespace E_Commerce.Common.Persistence.Services;

public class TenantService : ITenantService
{
    private TenantId? _tenantId;

    public TenantId TenantId => _tenantId ?? throw new InvalidOperationException("Tenant ID not set");

    public void SetTenant(TenantId tenantId)
    {
        _tenantId = tenantId;
    }
}