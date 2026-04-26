using HMS.Application.Abstractions.Persistence;
using HMS.Domain.Entities.PatientIntake;
using HMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using HMS.Domain.Enums;
public class AssignmentService : IAssignmentService
{
    private readonly IApplicationDbContext _context;

    public AssignmentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(Guid roomId, Guid doctorId)> AssignAsync(PatientIntake intake, CancellationToken ct)
    {
        var roomType = MapToRoomType(intake.VisitType);

        // 🛏️ اختار روم فاضية
        var room = await _context.Rooms
     .Where(r =>
         r.BranchId == intake.BranchId &&
         r.Type == roomType &&
         !_context.RoomAssignments.Any(a =>
             a.RoomId == r.Id && a.IsActive))
     .FirstOrDefaultAsync(ct);

        if (room == null)
            throw new Exception("No available rooms");

        // 👨‍⚕️ اختار دكتور (بسيطة دلوقتي)
        var doctor = await _context.Users
     .FirstOrDefaultAsync(ct); // مؤقت بس

        if (doctor == null)
            throw new Exception("No available doctors");

        return (room.Id, doctor.Id);
    }

    private RoomType MapToRoomType(VisitType type)
    {
        return type switch
        {
            VisitType.Outpatient => RoomType.Clinic,
            VisitType.Emergency => RoomType.ER,
            VisitType.Inpatient => RoomType.ICU,
            _ => RoomType.Clinic
        };
    }
}