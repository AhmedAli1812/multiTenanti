using HMS.SharedKernel.Primitives;
using HMS.Visits.Domain.Events;
using MediatR;

namespace HMS.Notifications.Application.EventHandlers;

/// <summary>
/// Handles VisitCreatedEvent — pushes a real-time notification to the dashboard.
///
/// Architecture note:
///   This handler sits in the Notifications module and subscribes to the
///   VisitCreatedEvent published by the Visits module after a visit is created.
///   The Notifications module knows about the event contract (domain event record)
///   but knows nothing about Visit internals — clean boundary.
/// </summary>
public sealed class VisitCreatedNotificationHandler(
    INotificationsService notificationsService)
    : INotificationHandler<VisitCreatedEvent>
{
    public async Task Handle(VisitCreatedEvent evt, CancellationToken cancellationToken)
    {
        var message = new NotificationMessage(
            Type:      "VisitCreated",
            TenantId:  evt.TenantId,
            Payload: new
            {
                VisitId    = evt.VisitId,
                PatientId  = evt.PatientId,
                BranchId   = evt.BranchId,
                OccurredOn = evt.OccurredOn
            });

        await notificationsService.BroadcastToTenantAsync(message, cancellationToken);
    }
}

/// <summary>
/// Handles RoomAssignedEvent — notifies reception that a room has been allocated.
/// </summary>
public sealed class RoomAssignedNotificationHandler(
    INotificationsService notificationsService)
    : INotificationHandler<HMS.Rooms.Domain.Events.RoomAssignedEvent>
{
    public async Task Handle(
        HMS.Rooms.Domain.Events.RoomAssignedEvent evt,
        CancellationToken cancellationToken)
    {
        var message = new NotificationMessage(
            Type:      "RoomAssigned",
            TenantId:  evt.TenantId,
            Payload: new
            {
                RoomId   = evt.RoomId,
                VisitId  = evt.VisitId,
                TenantId = evt.TenantId
            });

        await notificationsService.BroadcastToTenantAsync(message, cancellationToken);
    }
}
