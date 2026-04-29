using HMS.Notifications.Application;
using Microsoft.AspNetCore.SignalR;

namespace HMS.Notifications.Infrastructure;

/// <summary>
/// SignalR implementation of INotificationsService.
/// Routes messages to tenant-scoped groups so each tenant only receives
/// their own notifications.
/// </summary>
public sealed class SignalRNotificationsService(
    IHubContext<HMS.Infrastructure.RealTime.DashboardHub> dashboardHub,
    IHubContext<HMS.Infrastructure.RealTime.NotificationHub> notificationHub)
    : INotificationsService
{
    public async Task BroadcastToTenantAsync(
        NotificationMessage message,
        CancellationToken   ct = default)
    {
        var groupName = $"tenant-{message.TenantId}";

        // Push to DashboardHub (reception screen updates)
        await dashboardHub.Clients
            .Group(groupName)
            .SendAsync(message.Type, message.Payload, ct);

        // Push to NotificationHub (general alerts)
        await notificationHub.Clients
            .Group(groupName)
            .SendAsync("Notification", new
            {
                Type      = message.Type,
                TenantId  = message.TenantId,
                Payload   = message.Payload,
                Timestamp = DateTime.UtcNow
            }, ct);
    }
}
