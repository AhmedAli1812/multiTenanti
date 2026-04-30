using Microsoft.AspNetCore.SignalR;
using HMS.Application.Abstractions.Services;

namespace HMS.Infrastructure.RealTime;

public class DashboardNotifier : IDashboardNotifier
{
    private readonly IHubContext<NotificationHub> _hub;

    public DashboardNotifier(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }

    public async Task NotifyDashboardUpdated(Guid tenantId)
    {
        await _hub.Clients.Group($"tenant-{tenantId}")
            .SendAsync("dashboardUpdated", new
            {
                type = "NEW_VISIT",
                timestamp = DateTime.UtcNow
            });
    }

    // 🔥 Advanced Events
    public async Task NotifyNewVisit(Guid tenantId, Guid branchId)
    {
        // Notify Reception
        await _hub.Clients.Group($"Reception_{tenantId}_{branchId}")
            .SendAsync("visitCreated");

        // Notify Nurses (List Update)
        await _hub.Clients.Group($"Nurses_{tenantId}_{branchId}")
            .SendAsync("NewPatientAdded");

        // Notify Nurses (Toast/Bell)
        await _hub.Clients.Group($"Nurses_{tenantId}_{branchId}")
            .SendAsync("Notification", new
            {
                Type = "NEW_PATIENT",
                Payload = new { patientName = "مريض جديد" },
                Timestamp = DateTime.UtcNow
            });
    }

    public async Task NotifyDoctorQueue(Guid doctorId)
    {
        await _hub.Clients.Group($"Doctor_{doctorId}")
            .SendAsync("queueUpdated");
    }

    public async Task NotifyRoomAssigned(Guid tenantId, Guid branchId)
    {
        await _hub.Clients.Group($"Nurses_{tenantId}_{branchId}")
            .SendAsync("roomAssigned");
    }
    public async Task NotifyRoomStatusChanged(Guid tenantId, Guid branchId)
    {
        // Notify both sides
        await _hub.Clients.Group($"Nurses_{tenantId}_{branchId}")
            .SendAsync("roomStatusUpdated", new { timestamp = DateTime.UtcNow });
            
        await _hub.Clients.Group($"Reception_{tenantId}_{branchId}")
            .SendAsync("roomStatusUpdated", new { timestamp = DateTime.UtcNow });
    }
}