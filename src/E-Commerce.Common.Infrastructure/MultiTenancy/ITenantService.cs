using E_Commerce.Common.Domain.ValueObjects;

namespace E_Commerce.Common.Infrastructure.MultiTenancy;

public interface ITenantService
{
    TenantId TenantId { get; }
    void SetTenant(TenantId tenantId);
}