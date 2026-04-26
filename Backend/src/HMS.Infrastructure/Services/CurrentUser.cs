using HMS.Application.Abstractions.CurrentUser;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // =========================
    // 👤 UserId
    // =========================
    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null)
                return Guid.Empty;

            var claim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null || string.IsNullOrWhiteSpace(claim.Value))
                return Guid.Empty;

            return Guid.TryParse(claim.Value, out var id)
                ? id
                : Guid.Empty;
        }
    }

    // =========================
    // 🏢 TenantId
    // =========================
    public Guid TenantId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null)
                return Guid.Empty;

            var claim =
                user.FindFirst("tenantId") ??
                user.FindFirst("tenant_id") ??
                user.FindFirst("TenantId") ??
                user.FindFirst("tid");

            if (claim == null || string.IsNullOrWhiteSpace(claim.Value))
                return Guid.Empty;

            return Guid.TryParse(claim.Value, out var tenantId)
                ? tenantId
                : Guid.Empty;
        }
    }

    // =========================
    // 🌍 IsGlobal
    // =========================
    public bool IsGlobal =>
        _httpContextAccessor.HttpContext?.User?
            .FindFirst("isGlobal")?.Value == "true";

    // =========================
    // 👤 Role
    // =========================
    public string Role =>
        _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
}