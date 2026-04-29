using HMS.Application.Abstractions.Persistence;
using HMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using PatientIntakeEntity = HMS.Domain.Entities.PatientIntake.PatientIntake;

namespace HMS.Application.Features.PatientIntake.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IApplicationDbContext _context;

    public AssignmentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(Guid? roomId, Guid? doctorId)> AssignAsync(
        PatientIntakeEntity intake,
        Guid? requestedRoomId = null,
        Guid? requestedDoctorId = null,
        CancellationToken ct = default)
    {
        // ─────────────────────────────────────────────────────────────────
        // Guards: both TenantId and BranchId must be set before assignment.
        //
        // NOTE: PatientIntake.TenantId shadows TenantEntity.TenantId with a
        // non-nullable Guid, so we only need to check for Guid.Empty (the
        // default unset value), not for null.
        // ─────────────────────────────────────────────────────────────────
        if (intake.TenantId == Guid.Empty)
            throw new InvalidOperationException(
                "Intake.TenantId must be set before calling AssignAsync.");

        if (intake.BranchId == Guid.Empty)
            throw new InvalidOperationException(
                "Intake.BranchId must be set before calling AssignAsync.");

        var tenantId = intake.TenantId;
        var branchId = intake.BranchId;
        var roomType  = MapToRoomType(intake.VisitType);

        // ─────────────────────────────────────────────────────────────────
        // 🛏️ Find an available Room.
        // ─────────────────────────────────────────────────────────────────
        if (intake.VisitType != VisitType.Inpatient && (requestedRoomId == null || requestedRoomId == Guid.Empty))
        {
            // For non-inpatient visits, we don't auto-assign room or doctor per user request.
            return (null, null);
        }

        var occupiedRoomIds = await _context.RoomAssignments
            .IgnoreQueryFilters()
            .Where(a =>
                a.TenantId == tenantId &&
                a.IsActive             &&
                a.IsDeleted == false)
            .Select(a => a.RoomId)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var roomQuery = _context.Rooms
            .IgnoreQueryFilters()
            .Where(r =>
                r.TenantId == tenantId &&
                r.BranchId == branchId &&
                r.IsDeleted == false   &&
                !r.IsOccupied          &&
                (r.CleaningUntil == null || r.CleaningUntil <= now) &&
                !occupiedRoomIds.Contains(r.Id));

        if (requestedRoomId.HasValue && requestedRoomId.Value != Guid.Empty)
        {
            roomQuery = roomQuery.Where(r => r.Id == requestedRoomId.Value);
        }
        else
        {
            roomQuery = roomQuery.Where(r => r.Type == roomType).OrderBy(r => r.CreatedAt);
        }

        var room = await roomQuery.FirstOrDefaultAsync(ct);

        if (room == null)
        {
            if (intake.VisitType == VisitType.Inpatient)
            {
                throw new InvalidOperationException(
                    requestedRoomId.HasValue && requestedRoomId.Value != Guid.Empty
                        ? $"Requested room is unavailable or not found in branch {branchId}."
                        : $"No available {roomType} room found in branch {branchId}.");
            }
            // For others, if we got here (e.g. requestedRoomId was set), we just return nulls if not found
            return (null, null);
        }

        // IsAvailable() also checks CleaningUntil
        if (!room.IsAvailable())
            throw new InvalidOperationException(
                $"Room {room.RoomNumber} is unavailable (cleaning or already occupied).");

        room.Assign();

        // ─────────────────────────────────────────────────────────────────
        // 👨‍⚕️ Find an available Doctor.
        // ─────────────────────────────────────────────────────────────────
        var doctorRoleId = await _context.Roles
            .IgnoreQueryFilters()
            .Where(r => r.Name == "Doctor")
            .Select(r => r.Id)
            .FirstOrDefaultAsync(ct);

        if (doctorRoleId == Guid.Empty)
            throw new InvalidOperationException("Doctor role not found in the system.");

        var doctorQuery = _context.Users
            .IgnoreQueryFilters()
            .Where(u =>
                u.TenantId  == tenantId &&
                u.BranchId  == branchId &&
                u.IsActive             &&
                u.IsDeleted == false)
            .Where(u => _context.UserRoles
                .IgnoreQueryFilters()
                .Any(ur => ur.UserId == u.Id && ur.RoleId == doctorRoleId))
            .Select(u => u.Id);

        if (requestedDoctorId.HasValue && requestedDoctorId.Value != Guid.Empty)
        {
            doctorQuery = doctorQuery.Where(id => id == requestedDoctorId.Value);
        }
        else
        {
            // Auto-assign random doctor only if Inpatient
            doctorQuery = doctorQuery.OrderBy(_ => Guid.NewGuid());
        }

        var doctorUserId = await doctorQuery.FirstOrDefaultAsync(ct);

        if (doctorUserId == default)
        {
            if (intake.VisitType == VisitType.Inpatient)
            {
                throw new InvalidOperationException(
                    requestedDoctorId.HasValue && requestedDoctorId.Value != Guid.Empty
                        ? $"Requested doctor is unavailable or not found for branch {branchId} in tenant {tenantId}."
                        : $"No available doctor found for branch {branchId} in tenant {tenantId}.");
            }
            return (room.Id, null);
        }

        return (room.Id, doctorUserId);
    }

    // ─────────────────────────────────────────────────────────────────
    // Mapping: VisitType → RoomType
    // ─────────────────────────────────────────────────────────────────
    private static RoomType MapToRoomType(VisitType type) => type switch
    {
        VisitType.Outpatient => RoomType.Clinic,
        VisitType.Emergency  => RoomType.ER,
        VisitType.Inpatient  => RoomType.ICU,
        _                    => RoomType.Clinic
    };
}