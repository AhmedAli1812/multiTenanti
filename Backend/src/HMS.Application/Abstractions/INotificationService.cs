using HMS.Domain.Entities;

public interface INotificationService
{
    Task SendAsync(Notification notification, CancellationToken cancellationToken = default);

    // 🔥 مهم
    Task SendToRoleAsync(string role, string title, string message, CancellationToken cancellationToken = default);
}