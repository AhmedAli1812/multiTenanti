
using HMS.Application.Abstractions.Tenant;
using HMS.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace HMS.API.Middlewares
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
            HttpContext context,
            AuditLogService auditService,
            ITenantProvider tenantProvider)
        {
            // =========================
            // 🔥 Tenant (SAFE)
            // =========================
            var tenantId = tenantProvider.TryGetTenantId();

            // =========================
            // 🔥 User Info
            // =========================
            var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userGuid = null;

            if (Guid.TryParse(userId, out var parsedUserId))
            {
                userGuid = parsedUserId;
            }

            var userName =
                context.User?.Identity?.Name ??
                context.User?.FindFirst(ClaimTypes.Name)?.Value ??
                "Anonymous";

            // =========================
            // 🌐 Request Info
            // =========================
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var entityId = context.Request.RouteValues["id"]?.ToString() ?? "N/A";

            string? error = null;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                throw;
            }

            // =========================
            // 📊 Audit فقط للعمليات المهمة
            // =========================
            if (context.Request.Method == "POST" ||
                context.Request.Method == "PUT" ||
                context.Request.Method == "DELETE")
            {
                await auditService.LogAsync(
                    tenantId,
                    userGuid,
                    userName,
                    context.Request.Method,
                    context.Request.Path,
                    entityId,
                    null,
                    null,
                    ip
                );
            }
        }
    }
}

