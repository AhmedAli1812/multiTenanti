using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Abstractions.Persistence;
using HMS.Domain.Entities.Operations;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class AssignRoomHandler : IRequestHandler<AssignRoomCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public AssignRoomHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(AssignRoomCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId;

        // =========================
        // 🔥 Transaction
        // =========================
        using var tx = await _context.BeginTransactionAsync(cancellationToken);

        // =========================
        // 🔍 Get Visit
        // =========================
        var visit = await _context.Visits
            .FirstOrDefaultAsync(x =>
                x.Id == request.VisitId &&
                x.TenantId == tenantId,
                cancellationToken);

        if (visit == null)
            throw new InvalidOperationException("Visit not found");

        // =========================
        // 🔍 Get Room
        // =========================
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r =>
                r.Id == request.RoomId &&
                r.TenantId == tenantId,
                cancellationToken);

        if (room == null)
            throw new InvalidOperationException("Room not found");

        // =========================
        // 🚫 Room availability
        // =========================
        if (!room.IsAvailable())
            throw new InvalidOperationException("Room is not available");

        // =========================
        // 🔥 Close old assignments
        // =========================
        var activeAssignments = await _context.RoomAssignments
            .Where(a =>
                a.VisitId == visit.Id &&
                a.IsActive &&
                a.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        foreach (var a in activeAssignments)
        {
            a.IsActive = false;
            a.ReleasedAt = DateTime.UtcNow;

            // 🔥 release old room
            var oldRoom = await _context.Rooms
                .FirstOrDefaultAsync(r =>
                    r.Id == a.RoomId &&
                    r.TenantId == tenantId,
                    cancellationToken);

            oldRoom?.Release();
        }

        // =========================
        // 💣 Assign new room
        // =========================
        room.Assign();

        _context.RoomAssignments.Add(new RoomAssignment
        {
            Id = Guid.NewGuid(),
            VisitId = visit.Id,
            RoomId = room.Id,
            IsActive = true,
            TenantId = tenantId,
            AssignedAt = DateTime.UtcNow
        });

        // =========================
        // 💾 Save
        // =========================
        await _context.SaveChangesAsync(cancellationToken);

        await tx.CommitAsync(cancellationToken);
    }
}