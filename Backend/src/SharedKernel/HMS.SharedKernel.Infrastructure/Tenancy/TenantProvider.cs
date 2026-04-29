using Microsoft.AspNetCore.Http;

namespace HMS.SharedKernel.Infrastructure.Tenancy;

/// <summary>
/// Resolves the active TenantId from the current HTTP request.
///
/// Priority order (matches frontend auth.ts + backend JwtService):
///   1. HttpContext.Items["HMS_TenantId"]   — set by TenantMiddleware from header
///   2. X-Tenant-Id request header
///   3. JWT claim "orgId"                   — FIX: was "tenantId" — now unified
///   4. JWT claim "tenantId"                — fallback for backwards compat
///
/// Background jobs / seeders: call SetTenantId(guid) on IServiceScope context
/// using HttpContext.Items["HMS_TenantId"] BEFORE any DB operation.
/// </summary>
public sealed class TenantProvider : ITenantProvider
{
    // Key used in both HttpContext.Items and the fallback in-memory slot
    internal const string ItemsKey = "HMS_TenantId";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _inMemoryTenantId; // for background jobs / tests

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TryGetTenantId()
    {
        // 1. In-memory override (background jobs / tests)
        if (_inMemoryTenantId.HasValue)
            return _inMemoryTenantId;

        var ctx = _httpContextAccessor.HttpContext;
        if (ctx is null) return null;

        // 2. Already resolved by TenantMiddleware
        if (ctx.Items.TryGetValue(ItemsKey, out var cached) && cached is Guid g)
            return g;

        // 3. X-Tenant-Id header
        if (ctx.Request.Headers.TryGetValue("X-Tenant-Id", out var headerVal)
            && Guid.TryParse(headerVal, out var headerGuid))
        {
            ctx.Items[ItemsKey] = headerGuid;
            return headerGuid;
        }

        // 4. JWT claims — try "orgId" first, then legacy "tenantId"
        var user = ctx.User;
        var claimVal =
            user.FindFirst("orgId")?.Value ??
            user.FindFirst("tenantId")?.Value ??
            user.FindFirst("tenant_id")?.Value ??
            user.FindFirst("TenantId")?.Value;

        if (!string.IsNullOrEmpty(claimVal) && Guid.TryParse(claimVal, out var claimGuid))
        {
            ctx.Items[ItemsKey] = claimGuid;
            return claimGuid;
        }

        return null;
    }

    public bool IsSuperAdmin()
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx is null) return false;
        
        return ctx.User.FindFirst("isGlobal")?.Value == "true" 
               || ctx.User.IsInRole("Super Admin") 
               || ctx.User.IsInRole("SuperAdmin");
    }

    public void SetTenantId(Guid tenantId)
    {
        // For background jobs: write to both in-memory slot and HttpContext.Items
        // so both TryGetTenantId() paths resolve correctly.
        _inMemoryTenantId = tenantId;

        var ctx = _httpContextAccessor.HttpContext;
        if (ctx is not null)
            ctx.Items[ItemsKey] = tenantId;
    }
}
