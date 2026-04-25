using HMS.Application.Abstractions.Persistence;
using HMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetNurseRoomsHandler : IRequestHandler<GetNurseRoomsQuery, List<NurseRoomDto>>
{
    private readonly IApplicationDbContext _context;

    public GetNurseRoomsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NurseRoomDto>> Handle(GetNurseRoomsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // =============================
        // 🔥 Load active room assignments + visits
        // =============================
        var activeAssignments = await _context.RoomAssignments
            .AsNoTracking()
            .Where(a => a.IsActive)
            .Join(_context.Visits,
                a => a.VisitId,
                v => v.Id,
                (a, v) => new { a, v })
            .Where(x =>
                x.v.BranchId == request.BranchId &&
                x.v.Status != VisitStatus.Completed)
            .OrderByDescending(x => x.v.CreatedAt)
            .Select(x => new
            {
                RoomId = x.a.RoomId,
                VisitId = x.v.Id,
                PatientName = x.v.Patient.FullName
            })
            .ToListAsync(cancellationToken);

        var visitLookup = activeAssignments
            .GroupBy(v => v.RoomId)
            .ToDictionary(g => g.Key, g => g.First());

        // =============================
        // 🏥 Rooms
        // =============================
        var rooms = await _context.Rooms
            .AsNoTracking()
            .Where(r => r.BranchId == request.BranchId)
            .Select(r => new NurseRoomDto
            {
                RoomId = r.Id,
                RoomNumber = r.RoomNumber,
                IsOccupied = r.IsOccupied,
                CleaningUntil = r.CleaningUntil,

                Status =
                    r.CleaningUntil != null && r.CleaningUntil > now
                        ? "Cleaning"
                        : r.IsOccupied
                            ? "Occupied"
                            : "Available",

                CurrentVisitId = null,
                PatientName = null
            })
            .ToListAsync(cancellationToken);

        // =============================
        // 🔥 Merge
        // =============================
        foreach (var room in rooms)
        {
            if (visitLookup.TryGetValue(room.RoomId, out var visit))
            {
                room.CurrentVisitId = visit.VisitId;
                room.PatientName = visit.PatientName;
            }
        }

        return rooms;
    }
}