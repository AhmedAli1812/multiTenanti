namespace HMS.SharedKernel.Infrastructure.Tenancy;

/// <summary>
/// Tenant resolution abstraction.
/// Implementation: TenantProvider (reads HTTP context, JWT claims, headers).
/// </summary>
public interface ITenantProvider
{
    Guid? TryGetTenantId();
    bool  IsSuperAdmin();
    void  SetTenantId(Guid tenantId);
}
