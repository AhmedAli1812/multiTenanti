using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HMS.Infrastructure.RealTime;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var user = Context.User;

        if (user == null)
        {
            await base.OnConnectedAsync();
            return;
        }

        var tenantId = 
            user.FindFirst("orgId")?.Value ?? 
            user.FindFirst("tenantId")?.Value ??
            user.FindFirst("tenant_id")?.Value;

        var branchId = user.FindFirst("branchId")?.Value;
        var role = user.FindFirst(ClaimTypes.Role)?.Value;
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // =========================
        // 🔥 Tenant Group (IMPORTANT)
        // =========================
        if (!string.IsNullOrEmpty(tenantId))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"tenant-{tenantId}");
        }

        // =========================
        // 🏥 Branch Groups
        // =========================
        if (!string.IsNullOrEmpty(branchId))
        {
            // Nurses (and Admin/Reception for testing dashboard)
            if (role == "Nurse" || role == "Admin" || role == "Reception")
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"Nurses_{tenantId}_{branchId}");
            }

            // Reception
            if (role == "Reception" || role == "Admin")
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"Reception_{tenantId}_{branchId}");
            }
        }

        // =========================
        // 👨‍⚕️ Doctor Group (QUEUE 🔥)
        // =========================
        if (role == "Doctor" && !string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"Doctor_{userId}");
        }

        await base.OnConnectedAsync();
    }
}