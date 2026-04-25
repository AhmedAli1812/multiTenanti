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

    public Guid UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return string.IsNullOrEmpty(userId)
                ? Guid.Empty
                : Guid.Parse(userId);
        }
    }

    public Guid TenantId
    {
        get
        {
            var tenantId = _httpContextAccessor.HttpContext?.User?
                .FindFirst("tenantId")?.Value;

            return string.IsNullOrEmpty(tenantId)
                ? Guid.Empty
                : Guid.Parse(tenantId);
        }
    }

    public string Role
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}