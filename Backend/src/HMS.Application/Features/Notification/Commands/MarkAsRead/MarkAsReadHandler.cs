using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class MarkAsReadHandler : IRequestHandler<MarkAsReadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public MarkAsReadHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        // =========================
        // 💣 Validation
        // =========================
        if (request.NotificationId == Guid.Empty)
            throw new ArgumentException("Invalid notification");

        var userId = _currentUser.UserId;
        var tenantId = _currentUser.TenantId;

        // =========================
        // 🔥 Get Notification (Secure)
        // =========================
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(x =>
                x.Id == request.NotificationId &&
                x.UserId == userId &&           // 💣 مهم جدًا
                x.TenantId == tenantId,         // 💣 SaaS
                cancellationToken);

        if (notification == null)
            throw new InvalidOperationException("Notification not found");

        // =========================
        // 🔥 Update
        // =========================
        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}