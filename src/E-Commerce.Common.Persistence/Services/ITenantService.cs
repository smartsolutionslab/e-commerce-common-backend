using E_Commerce.Common.Domain.ValueObjects;

namespace E_Commerce.Common.Persistence.Services;

public interface ITenantService
{
    TenantId? GetCurrentTenant();
    void SetTenant(TenantId tenantId);
}