using HMS.SharedKernel.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HMS.SharedKernel.Infrastructure.CurrentUser;

/// <summary>
/// SharedKernel ICurrentUser implementation used by the AuditAndDomainEventInterceptor.
/// Reads claims directly from IHttpContextAccessor.
/// </summary>
public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(
            Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Principal?.FindFirst("sub")?.Value,
            out var id) ? id : null;

    public Guid? TenantId
    {
        get
        {
            var raw =
                Principal?.FindFirst("orgId")?.Value
                ?? Principal?.FindFirst("tenantId")?.Value;

            return Guid.TryParse(raw, out var tid) ? tid : null;
        }
    }

    public bool IsGlobal =>
        bool.TryParse(Principal?.FindFirst("isGlobal")?.Value, out var g) && g;
}
