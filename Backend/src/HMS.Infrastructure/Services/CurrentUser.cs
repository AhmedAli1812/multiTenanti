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

    // =========================
    // 🔥 UserId
    // =========================
    public Guid UserId =>
        Guid.TryParse(
            User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User?.FindFirst("sub")?.Value,   // fallback
            out var userId)
        ? userId
        : Guid.Empty;

    // =========================
    // 🔥 TenantId
    // =========================
    public Guid TenantId =>
        Guid.TryParse(
            User?.FindFirst("tenantId")?.Value,
            out var tenantId)
        ? tenantId
        : Guid.Empty;

    // =========================
    // 💣 BranchId
    // =========================
    public Guid? BranchId
    {
        get
        {
            var value = User?.FindFirst("branchId")?.Value;

            return Guid.TryParse(value, out var branchId)
                ? branchId
                : null;
        }
    }

    // =========================
    // 💣 Roles (supports multi roles)
    // =========================
    public List<string> Roles =>
        User?
            .FindAll(ClaimTypes.Role)
            .Select(r => r.Value)
            .ToList()
        ?? new List<string>();

    // =========================
    // 💣 Single Role (optional helper)
    // =========================
    public string? Role => Roles.FirstOrDefault();

    // =========================
    // 🔥 IsGlobal
    // =========================
    public bool IsGlobal =>
        bool.TryParse(
            User?.FindFirst("isGlobal")?.Value,
            out var isGlobal)
        && isGlobal;
}