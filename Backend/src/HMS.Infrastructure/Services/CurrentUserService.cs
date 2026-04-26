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
        // 🏢 IsGlobal 🔥
        // =========================
        public bool IsGlobal =>
            User.FindFirst("isGlobal")?.Value == "true";

        // =========================
        // 🏢 TenantId (SAFE 🔥)
        // =========================
        public Guid TenantId
        {
            get
            {
                var tenantClaim = User.FindFirst("tenantId")
                                  ?? User.FindFirst("tenant_id")
                                  ?? User.FindFirst("TenantId")
                                  ?? User.FindFirst("tid");

                // ✅ لو مش موجود → Global Admin
                if (tenantClaim == null || string.IsNullOrWhiteSpace(tenantClaim.Value))
                {
                    if (IsGlobal)
                        return Guid.Empty; // 💣 مهم

                    throw new UnauthorizedAccessException("TenantId not found");
                }

                if (!Guid.TryParse(tenantClaim.Value, out var tenantId))
                    throw new UnauthorizedAccessException("Invalid TenantId");

                return tenantId;
            }
        }
    }
}