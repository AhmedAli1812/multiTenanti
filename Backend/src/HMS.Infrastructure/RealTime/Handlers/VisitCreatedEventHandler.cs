using HMS.Application.Features.Visits.Events;
using HMS.Infrastructure.RealTime;
using MediatR;
using Microsoft.AspNetCore.SignalR;

public class VisitCreatedEventHandler : INotificationHandler<VisitCreatedEvent>
{
    private readonly IHubContext<NotificationHub> _hub;

    public VisitCreatedEventHandler(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }

    public async Task Handle(VisitCreatedEvent notification, CancellationToken cancellationToken)
    {
        var groupName = $"Nurses_{notification.TenantId}_{notification.BranchId}";

        var payload = new
        {
            visitId = notification.VisitId,
            roomId = notification.RoomId,
            patientId = notification.PatientId,
            createdAt = DateTime.UtcNow
        };

        try
        {
            await _hub.Clients
                .Group(groupName)
                .SendAsync("NewVisitAssigned", payload, cancellationToken);
        }
        catch (Exception)
        {
            // ❗ متكسرش السيستم بسبب SignalR
            // ممكن تضيف logging هنا
        }
    }
}