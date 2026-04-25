using HMS.Application.Abstractions.Tenant;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HMS.Infrastructure.Services
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetTenantId()
        {
            var tenant = TryGetTenantId();

            if (!tenant.HasValue)
                throw new Exception("Tenant not found in request");

            return tenant;
        }

        public Guid? TryGetTenantId()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return null;

            // =========================
            // 🔥 1. HEADER (أعلى أولوية)
            // =========================
            var headerTenant = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

            if (!string.IsNullOrEmpty(headerTenant) &&
                Guid.TryParse(headerTenant, out var headerGuid))
            {
                return headerGuid;
            }

            // =========================
            // 🔥 2. JWT
            // =========================
            var claim = context.User?.FindFirst("orgId")?.Value;

            if (!string.IsNullOrEmpty(claim) &&
                Guid.TryParse(claim, out var claimGuid))
            {
                return claimGuid;
            }

            return null;
        }

        public void SetTenantId(Guid tenantId)
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return;

            context.Items["TenantId"] = tenantId;
        }

        public bool IsSuperAdmin()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return false;

            return context.User?.IsInRole("Super Admin") ?? false;
        }
    }
}