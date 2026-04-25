using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Services;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Notifications.Commands.SendNotification
{
    public class SendNotificationHandler : IRequestHandler<SendNotificationCommand>
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly ICurrentUser _currentUser;

        public SendNotificationHandler(
            IApplicationDbContext context,
            INotificationService notificationService,
            ICurrentUser currentUser)
        {
            _context = context;
            _notificationService = notificationService;
            _currentUser = currentUser;
        }

        public async Task Handle(SendNotificationCommand request, CancellationToken cancellationToken)
        {
            // =========================
            // 💣 Validation
            // =========================
            if (request.UserId == Guid.Empty)
                throw new ArgumentException("Invalid user");

            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Title is required");

            if (string.IsNullOrWhiteSpace(request.Message))
                throw new ArgumentException("Message is required");

            var tenantId = _currentUser.TenantId;

            var title = request.Title.Trim();
            var message = request.Message.Trim();

            // =========================
            // 🔥 Check user exists in same tenant
            // =========================
            var userExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == request.UserId && u.TenantId == tenantId, cancellationToken);

            if (!userExists)
                throw new InvalidOperationException("User not found");

            // =========================
            // 🔥 Create Notification
            // =========================
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Title = title,
                Message = message,
                Type = request.Type,
                ReferenceId = request.ReferenceId,
                ReferenceType = request.ReferenceType,
                TenantId = tenantId,          // 💣 مهم جدًا
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _context.Notifications.AddAsync(notification, cancellationToken);

            // =========================
            // 💾 Save first
            // =========================
            await _context.SaveChangesAsync(cancellationToken);

            // =========================
            // 🔔 Send (Realtime / Push)
            // =========================
            await _notificationService.SendAsync(notification);
        }
    }
}