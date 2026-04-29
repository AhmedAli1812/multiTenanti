namespace HMS.Notifications.Application;

/// <summary>Notification message dispatched to connected clients.</summary>
public sealed record NotificationMessage(
    string Type,
    Guid   TenantId,
    object Payload);

/// <summary>
/// Abstraction for real-time notification broadcast.
/// Implemented by the Infrastructure layer (SignalR).
/// </summary>
public interface INotificationsService
{
    Task BroadcastToTenantAsync(
        NotificationMessage message,
        CancellationToken   ct = default);
}
