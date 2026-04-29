using HMS.Application.Abstractions.Tenant;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HMS.Infrastructure.Services;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Used as the Items key so the literal string is not scattered
    private const string ItemsKey = "HMS_TenantId";

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // ─────────────────────────────────────────────────────────────────
    // GetTenantId — throws if not found (use in authenticated endpoints)
    // ─────────────────────────────────────────────────────────────────
    public Guid? GetTenantId()
    {
        var tenant = TryGetTenantId();

        // Super Admins are allowed to have no tenant context (Global access)
        if (!tenant.HasValue && !IsSuperAdmin())
            throw new UnauthorizedAccessException(
                "TenantId could not be resolved from the current request context.");

        return tenant;
    }

    // ─────────────────────────────────────────────────────────────────
    // TryGetTenantId — resolution priority:
    //
    //   1. HttpContext.Items["HMS_TenantId"]  ← set by SetTenantId()
    //      Used by background services / seed jobs that call SetTenantId
    //      before performing queries.
    //
    //   2. X-Tenant-Id request header  ← Super Admin header override
    //
    //   3. JWT claim (orgId → tenantId)  ← normal authenticated user
    //
    // This ordering ensures that:
    //   • SetTenantId() actually works (bug: previously written to Items
    //     but never read from there).
    //   • Header override takes precedence over JWT (Super Admin use-case).
    //   • JWT is the default for all regular users.
    // ─────────────────────────────────────────────────────────────────
    public Guid? TryGetTenantId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return null;

        // ── Priority 1: programmatic override (background service / seed) ──
        if (context.Items.TryGetValue(ItemsKey, out var itemValue) &&
            itemValue is Guid itemGuid &&
            itemGuid != Guid.Empty)
        {
            return itemGuid;
        }

        // ── Priority 2: X-Tenant-Id header (Super Admin cross-tenant) ──
        var headerTenant = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(headerTenant) &&
            Guid.TryParse(headerTenant, out var headerGuid))
        {
            return headerGuid;
        }

        // ── Priority 3: JWT claim ──
        var claim =
            context.User?.FindFirst("orgId")?.Value
            ?? context.User?.FindFirst("tenantId")?.Value;

        if (!string.IsNullOrEmpty(claim) &&
            Guid.TryParse(claim, out var claimGuid))
        {
            return claimGuid;
        }

        return null;
    }

    // ─────────────────────────────────────────────────────────────────
    // SetTenantId — explicitly sets the tenant for the current scope.
    //
    // FIXED: Previously wrote to context.Items["TenantId"] but
    // TryGetTenantId never read that key — the setter was dead code.
    // Now both use the same ItemsKey constant.
    //
    // Use this in:
    //   • Seeder jobs (no HTTP request, creates a stub HttpContext)
    //   • Background services that impersonate a tenant
    // ─────────────────────────────────────────────────────────────────
    public void SetTenantId(Guid tenantId)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return;

        context.Items[ItemsKey] = tenantId;
    }

    // ─────────────────────────────────────────────────────────────────
    // IsSuperAdmin — checks the JWT role claim
    // ─────────────────────────────────────────────────────────────────
    public bool IsSuperAdmin()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
            return false;

        return context.User?.IsInRole("Super Admin") == true 
               || context.User?.IsInRole("SuperAdmin") == true;
    }
}