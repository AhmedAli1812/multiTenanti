using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Services;
using HMS.Application.Abstractions.Tenant;
using HMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HMS.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IApplicationDbContext _context;
        private readonly IRealTimeNotifier _notifier;
        private readonly ITenantProvider _tenantProvider;

        public NotificationService(
            IApplicationDbContext context,
            IRealTimeNotifier notifier,
            ITenantProvider tenantProvider)
        {
            _context = context;
            _notifier = notifier;
            _tenantProvider = tenantProvider;
        }

        // =========================
        // 🔔 Send To Single User
        // =========================
        public async Task SendAsync(Notification notification, CancellationToken cancellationToken = default)
        {
            // 🔥 Tenant
            if (notification.TenantId == Guid.Empty)
                notification.TenantId = _tenantProvider.GetTenantId();

            // 🔥 Defaults
            notification.Id = notification.Id == Guid.Empty ? Guid.NewGuid() : notification.Id;
            notification.CreatedAt = notification.CreatedAt == default ? DateTime.UtcNow : notification.CreatedAt;
            notification.IsRead = false;

            // 🔥 Validation (Production Safety)
            if (notification.UserId == Guid.Empty)
                throw new Exception("UserId is required for notification");

            if (string.IsNullOrWhiteSpace(notification.Title))
                throw new Exception("Notification title is required");

            // 🔥 Save in DB
            await _context.Notifications.AddAsync(notification, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // 🔥 Real-time push
            await _notifier.SendToUserAsync(notification.UserId, new
            {
                notification.Id,
                notification.Title,
                notification.Message,
                notification.Type,
                notification.CreatedAt
            });
        }

        // =========================
        // 🔔 Send To Role (Group)
        // =========================
        public async Task SendToRoleAsync(string role, string title, string message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new Exception("Role is required");

            var tenantId = _tenantProvider.GetTenantId();

            // 🔥 Get all users in role
            var users = await (
    from u in _context.Users
    join ur in _context.UserRoles on u.Id equals ur.UserId
    join r in _context.Roles on ur.RoleId equals r.Id
    where r.Name == role && u.TenantId == tenantId
    select u.Id
).ToListAsync(cancellationToken);

            var notifications = new List<Notification>();

            foreach (var userId in users)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = title,
                    Message = message,
                    TenantId = tenantId,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                notifications.Add(notification);
            }

            // 🔥 Save bulk
            await _context.Notifications.AddRangeAsync(notifications, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // 🔥 Real-time (Group)
            await _notifier.SendToGroupAsync(role, new
            {
                title,
                message,
                createdAt = DateTime.UtcNow
            });
        }
    }
}