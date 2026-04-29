using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Abstractions.Services;
using HMS.Domain.Entities;
using HMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Visits.UpdateVisitStatus;

public class UpdateVisitStatusHandler : IRequestHandler<UpdateVisitStatusCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notification;
    private readonly ICurrentUser _currentUser;
    private readonly IDashboardNotifier _dashboard;

    public UpdateVisitStatusHandler(
        IApplicationDbContext context,
        INotificationService notification,
        ICurrentUser currentUser,
        IDashboardNotifier dashboard)
    {
        _context = context;
        _notification = notification;
        _currentUser = currentUser;
        _dashboard = dashboard;
    }

    public async Task<Unit> Handle(UpdateVisitStatusCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        using var tx = await _context.BeginTransactionAsync(ct);

        // =========================
        // 🔍 Get Visit
        // =========================
        var visit = await _context.Visits
            .FirstOrDefaultAsync(x =>
                x.Id == request.VisitId &&
                x.TenantId == tenantId,
                ct);

        if (visit == null)
            throw new InvalidOperationException("Visit not found");

        if (visit.Status == request.Status)
            return Unit.Value;

        // =========================
        // 💣 Change Status
        // =========================
        visit.ChangeStatus(request.Status);

        // =========================
        // 🔥 Get Active Assignment
        // =========================
        var assignment = await _context.RoomAssignments
            .FirstOrDefaultAsync(a =>
                a.VisitId == visit.Id &&
                a.IsActive &&
                a.TenantId == tenantId,
                ct);

        // =========================
        // 🛏️ ROOM SIDE EFFECTS
        // =========================

        // دخول العملية
        if (request.Status == VisitStatus.InOp)
        {
            if (assignment == null)
                throw new InvalidOperationException("Room not assigned");

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r =>
                    r.Id == assignment.RoomId &&
                    r.TenantId == tenantId,
                    ct);

            if (room == null)
                throw new InvalidOperationException("Room not found");

            if (!room.IsAvailable())
                throw new InvalidOperationException("Room is not available");

            room.Assign();

            await _dashboard.NotifyRoomAssigned(tenantId, visit.BranchId);
        }

        // انتهاء العملية أو انتهاء الزيارة بالكامل
        if (request.Status == VisitStatus.OpCompleted || request.Status == VisitStatus.Completed)
        {
            if (assignment != null)
            {
                var room = await _context.Rooms
                    .FirstOrDefaultAsync(r =>
                        r.Id == assignment.RoomId &&
                        r.TenantId == tenantId,
                        ct);
 
                if (room != null)
                {
                    room.Release();
 
                    // 🔥 realtime
                    await _dashboard.NotifyRoomAssigned(tenantId, visit.BranchId);
 
                    // 💣 cleaning timer
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromMinutes(15));
                        await _dashboard.NotifyRoomStatusChanged(tenantId, visit.BranchId);
                    });
                }
 
                assignment.IsActive = false;
                assignment.ReleasedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(ct);

        // =========================
        // 🔔 Notifications
        // =========================
        var patientName = await _context.Visits
            .Where(v => v.Id == visit.Id)
            .Select(v => v.Patient.FullName)
            .FirstAsync(ct);

        var message = $"Patient {patientName} status changed to {request.Status}";

        if (visit.DoctorId.HasValue)
        {
            await _notification.SendAsync(new Notification
            {
                UserId = visit.DoctorId.Value,
                Title = "Visit Status Updated",
                Message = message,
                TenantId = tenantId
            }, ct);
        }

        await _notification.SendToRoleAsync(
            "Nurse",
            "Visit Status Updated",
            message,
            ct
        );

        await tx.CommitAsync(ct);

        return Unit.Value;
    }
}