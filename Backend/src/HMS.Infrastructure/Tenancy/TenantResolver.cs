using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace HMS.Infrastructure.Tenancy;

public static class TenantResolver
{
    public static Guid? ResolveTenantId(HttpContext context)
    {
        var tenantClaim = context.User?.FindFirst("orgId");

        if (tenantClaim == null)
            return null;

        return Guid.Parse(tenantClaim.Value);
    }
}