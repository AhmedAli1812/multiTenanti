using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Abstractions.Services;
using HMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        // 💣 Change Status
        // =========================
        visit.ChangeStatus(VisitStatus.Completed);

        // =========================
        // 🔥 Get Room Assignment
        // =========================
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

                // 💣 Notify after cleaning time
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(15));
                    await _dashboard.NotifyRoomStatusChanged(tenantId, visit.BranchId);
                });
            }
        }

        await _context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Unit.Value;
    }
}