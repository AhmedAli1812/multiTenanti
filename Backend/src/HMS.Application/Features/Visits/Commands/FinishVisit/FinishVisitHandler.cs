using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Abstractions.Services;
using HMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Visits.Commands.FinishVisit;

public class FinishVisitHandler : IRequestHandler<FinishVisitCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IDashboardNotifier _dashboard;

    public FinishVisitHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IDashboardNotifier dashboard)
    {
        _context = context;
        _currentUser = currentUser;
        _dashboard = dashboard;
    }

    public async Task<Unit> Handle(FinishVisitCommand request, CancellationToken ct)
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

        if (visit.Status == VisitStatus.Completed)
            return Unit.Value;

        // =========================
        // 💣 Change Status (Two-Step Checkout for Inpatients)
        // =========================
        bool isNowCompleted = false;

        // 🔍 Check if patient is in a room (Ward)
        var hasRoom = await _context.RoomAssignments
            .AnyAsync(a => a.VisitId == visit.Id && a.IsActive && a.TenantId == tenantId, ct);

        if (hasRoom || visit.VisitType == VisitType.Inpatient)
        {
            // 1️⃣ If already in a pending state, the second click (from either side) completes it
            if (visit.Status == VisitStatus.PendingCheckoutNurse || 
                visit.Status == VisitStatus.PendingCheckoutReception)
            {
                visit.ChangeStatus(VisitStatus.Completed);
            }
            else 
            {
                // 2️⃣ First click: determine who is clicking
                var role = _currentUser.Role;
                
                if (string.Equals(role, "Nurse", StringComparison.OrdinalIgnoreCase))
                {
                    // Nurse clicked first -> Wait for Reception
                    visit.ChangeStatus(VisitStatus.PendingCheckoutReception);
                }
                else if (string.Equals(role, "Reception", StringComparison.OrdinalIgnoreCase))
                {
                    // Reception clicked first -> Wait for Nurse
                    visit.ChangeStatus(VisitStatus.PendingCheckoutNurse);
                }
                else if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) || 
                         string.Equals(role, "HospitalAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    // Admin clicked: Move to first pending state (Wait for Reception)
                    // unless they want a force-complete? 
                    // Let's make them move to pending so they see the red row first.
                    visit.ChangeStatus(VisitStatus.PendingCheckoutReception);
                }
                else
                {
                    // Unknown role: bypass for now (or complete)
                    visit.ChangeStatus(VisitStatus.Completed);
                }
            }
        }
        else
        {
            visit.ChangeStatus(VisitStatus.Completed);
        }

        isNowCompleted = visit.Status == VisitStatus.Completed;

        // Notification moved to after commit to avoid race conditions

        // =========================
        // 🔥 Get Room Assignment (Only if fully completed)
        // =========================
        if (isNowCompleted)
        {
        var assignment = await _context.RoomAssignments
            .FirstOrDefaultAsync(a =>
                a.VisitId == visit.Id &&
                a.IsActive &&
                a.TenantId == tenantId,
                ct);

        if (assignment != null)
        {
            assignment.IsActive = false;
            assignment.ReleasedAt = DateTime.UtcNow;

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r =>
                    r.Id == assignment.RoomId &&
                    r.TenantId == tenantId,
                    ct);

            if (room != null)
            {
                room.Release();

                // 🔥 Notify immediately
                await _dashboard.NotifyRoomAssigned(tenantId, visit.BranchId);
                await _dashboard.NotifyPatientDischarged(tenantId, visit.BranchId, visit.Id);

                // 💣 Notify after cleaning time
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(15));
                    await _dashboard.NotifyRoomStatusChanged(tenantId, visit.BranchId);
                });
            }
        }
        }

        await _context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        // 🔥 Broadcast signal AFTER commit so other party sees the NEW status
        await _dashboard.NotifyRoomStatusChanged(tenantId, visit.BranchId);

        return Unit.Value;
    }
}