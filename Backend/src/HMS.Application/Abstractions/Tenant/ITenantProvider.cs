
namespace HMS.Application.Abstractions.Tenant
{
    public interface ITenantProvider
    {
        Guid? GetTenantId();       // 🔴 strict
        Guid? TryGetTenantId();    // 🟢 safe
        void SetTenantId(Guid tenantId);
        bool IsSuperAdmin();
    }
}

