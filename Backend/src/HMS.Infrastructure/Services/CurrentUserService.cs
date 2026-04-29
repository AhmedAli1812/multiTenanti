using HMS.Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HMS.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal User =>
            _httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("No user context");

        // =========================
        // 👤 UserId
        // =========================
        public Guid UserId
        {
            get
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier);

                if (claim == null || !Guid.TryParse(claim.Value, out var id))
                    throw new UnauthorizedAccessException("Invalid UserId");

                return id;
            }
        }

        // =========================
        // 🌍 IsGlobal
        // =========================
        public bool IsGlobal =>
            User.FindFirst("isGlobal")?.Value == "true";

        // =========================
        // 🏢 TenantId (FIXED 🔥)
        // =========================
        public Guid TenantId
        {
            get
            {
                // 🔥 دعم كل الاحتمالات (بس الأهم orgId)
                var tenantClaim =
                    User.FindFirst("orgId") ??   // ✅ الأساسي
                    User.FindFirst("tenantId") ??
                    User.FindFirst("tenant_id") ??
                    User.FindFirst("TenantId") ??
                    User.FindFirst("tid");

                // ❌ مفيش tenant
                if (tenantClaim == null || string.IsNullOrWhiteSpace(tenantClaim.Value))
                {
                    if (IsGlobal)
                        throw new InvalidOperationException("Global user should not request TenantId");

                    throw new UnauthorizedAccessException("TenantId not found in token");
                }

                if (!Guid.TryParse(tenantClaim.Value, out var tenantId))
                    throw new UnauthorizedAccessException("Invalid TenantId");

                return tenantId;
            }
        }
    }
}