using HMS.Application.Abstractions.Tenant;
using HMS.Infrastructure.Tenancy;
using System.Security.Claims;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        // =========================
        // 🔐 فقط لو user authenticated
        // =========================
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            // 🔥 الأفضل تعتمد على Role
            var isSuperAdmin =
                context.User.IsInRole("Super Admin") 
                || context.User.IsInRole("SuperAdmin")
                || context.User.HasClaim("isGlobal", "true"); 

            // =========================
            // 🧠 Resolve Tenant
            // =========================
            var tenantId = TenantResolver.ResolveTenantId(context);

            if (tenantId.HasValue)
            {
                tenantProvider.SetTenantId(tenantId.Value);
            }
            else
            {
                // 👇 لو مش Super Admin → لازم tenant
                if (!isSuperAdmin)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Tenant ID is required");
                    return;
                }

                // 👇 Super Admin → نعدّي بدون Tenant
                // مفيش SetTenantId
            }
        }

        await _next(context);
    }
}