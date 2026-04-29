using HMS.Application.Abstractions.CurrentUser;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HMS.Infrastructure.CurrentUser;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    // ─────────────────────────────────────────────────────────────────
    // 🔥 UserId
    //
    // Returns Guid.Empty if the token has no NameIdentifier/sub claim.
    // Callers that require a valid UserId must guard against Guid.Empty.
    // (Handler-level validation converts Guid.Empty → 401.)
    // ─────────────────────────────────────────────────────────────────
    public Guid UserId =>
        Guid.TryParse(
            User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User?.FindFirst("sub")?.Value,
            out var userId)
        ? userId
        : Guid.Empty;

    // ─────────────────────────────────────────────────────────────────
    // 🔥 TenantId
    //
    // FIXED: Previously returned Guid.Empty silently when no claim
    // existed. This caused all tenant-filtered queries to run against
    // TenantId = 00000000-... which could match nothing (correct) but
    // also produces misleading "not found" errors instead of 401s.
    //
    // Now checks: orgId → tenantId → tenant_id → TenantId → tid
    // These match exactly the claims emitted by JwtService.
    // Returns Guid.Empty only for IsGlobal users (they have no tenant).
    // ─────────────────────────────────────────────────────────────────
    public Guid TenantId
    {
        get
        {
            // Global admins intentionally carry no tenantId claim.
            if (IsGlobal)
                return Guid.Empty;

            var raw =
                User?.FindFirst("orgId")?.Value
                ?? User?.FindFirst("tenantId")?.Value
                ?? User?.FindFirst("tenant_id")?.Value
                ?? User?.FindFirst("TenantId")?.Value
                ?? User?.FindFirst("tid")?.Value;

            if (string.IsNullOrWhiteSpace(raw))
                throw new UnauthorizedAccessException(
                    "TenantId claim not found in the JWT token. " +
                    "Ensure the token was issued with an 'orgId' or 'tenantId' claim.");

            if (!Guid.TryParse(raw, out var tenantId))
                throw new UnauthorizedAccessException(
                    $"TenantId claim value '{raw}' is not a valid GUID.");

            return tenantId;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // 💣 BranchId (optional — not all users are branch-scoped)
    // ─────────────────────────────────────────────────────────────────
    public Guid? BranchId
    {
        get
        {
            var value = User?.FindFirst("branchId")?.Value;
            return Guid.TryParse(value, out var branchId) ? branchId : null;
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // 💣 Roles (multi-role support)
    // ─────────────────────────────────────────────────────────────────
    public List<string> Roles =>
        User?
            .FindAll(ClaimTypes.Role)
            .Select(r => r.Value)
            .ToList()
        ?? new List<string>();

    // ─────────────────────────────────────────────────────────────────
    // 💣 Single Role helper (first role wins)
    // ─────────────────────────────────────────────────────────────────
    public string? Role => Roles.FirstOrDefault();

    // ─────────────────────────────────────────────────────────────────
    // 🔥 IsGlobal
    // ─────────────────────────────────────────────────────────────────
    public bool IsGlobal =>
        bool.TryParse(
            User?.FindFirst("isGlobal")?.Value,
            out var isGlobal)
        && isGlobal;
}