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

        // =========================
        // 🔴 STRICT
        // =========================
        public Guid? GetTenantId()
        {
            var tenantId = TryGetTenantId();

            if (!tenantId.HasValue)
                throw new Exception("Tenant not found in request");

            return tenantId;
        }

        // =========================
        // 🟢 SAFE
        // =========================
        public Guid? TryGetTenantId()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return null;

            // 🔥 Super Admin → global access
            if (IsSuperAdmin())
                return null;

            // 🔥 من Middleware
            if (context.Items.TryGetValue("TenantId", out var tenantObj) && tenantObj is Guid tenantId)
            {
                return tenantId;
            }

            // 🔥 من JWT
            var claim = context.User?.FindFirst("orgId")?.Value;

            if (!string.IsNullOrEmpty(claim) && Guid.TryParse(claim, out var parsedTenant))
            {
                return parsedTenant;
            }

            return null;
        }

        // =========================
        // 💣 SET TENANT
        // =========================
        public void SetTenantId(Guid tenantId)
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return;

            context.Items["TenantId"] = tenantId;
        }

        // =========================
        // 🔥 SUPER ADMIN
        // =========================
        public bool IsSuperAdmin()
        {
            var context = _httpContextAccessor.HttpContext;

            if (context == null)
                return false;

            return context.User?.IsInRole("Super Admin") ?? false;
        }
    }
}