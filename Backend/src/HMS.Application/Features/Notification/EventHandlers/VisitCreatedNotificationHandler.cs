using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Services;
using HMS.Application.Features.Visits.Events;
using HMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Notifications.EventHandlers
{
    public class VisitCreatedNotificationHandler : INotificationHandler<VisitCreatedEvent>
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public VisitCreatedNotificationHandler(
            IApplicationDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task Handle(VisitCreatedEvent notification, CancellationToken cancellationToken)
        {
            // =========================
            // 💣 Validation
            // =========================
            if (notification.NurseId == Guid.Empty)
                return; // مفيش حد نبعتله

            // =========================
            // 🔥 Check nurse exists
            // =========================
            var nurseExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(x => x.Id == notification.NurseId && x.TenantId == notification.TenantId, cancellationToken);

            if (!nurseExists)
                return;

            // =========================
            // 🔥 Create Notification
            // =========================
            var entity = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = notification.NurseId,
                Title = "New Patient",
                Message = "Patient checked in",
                Type = "info", // 👈 ممكن تتحول enum بعدين
                ReferenceType = "Visit",
                ReferenceId = notification.VisitId,
                TenantId = notification.TenantId, // 💣 مهم جدًا
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _context.Notifications.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // =========================
            // 🔔 Send Realtime
            // =========================
            try
            {
                await _notificationService.SendAsync(entity);
            }
            catch
            {
                // 💣 ما نكسرش السيستم لو الريل تايم وقع
                // ممكن تضيف logging هنا بعدين
            }
        }
    }
}