using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Services;
using HMS.Application.Dtos;
using HMS.Application.Features.PatientIntake.Services;
using HMS.Application.Features.Reception.Intake.Commands;
using HMS.Domain.Entities.Operations;
using HMS.Domain.Entities.Patients;
using HMS.Domain.Entities.Visits;
using HMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PatientIntakeEntity = HMS.Domain.Entities.PatientIntake.PatientIntake;

namespace HMS.Application.Features.PatientIntake.Commands.SubmitIntake;

public class SubmitIntakeHandler : IRequestHandler<SubmitIntakeCommand, WristbandDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAssignmentService _assignment;
    private readonly IQrCodeService _qr;
    private readonly IDashboardNotifier _notifier;
    private readonly ICurrentUser _currentUser;

    public SubmitIntakeHandler(
        IApplicationDbContext context,
        IAssignmentService assignment,
        IQrCodeService qr,
        IDashboardNotifier notifier,
        ICurrentUser currentUser)
    {
        _context    = context;
        _assignment = assignment;
        _qr         = qr;
        _notifier   = notifier;
        _currentUser = currentUser;
    }

    public async Task<WristbandDto> Handle(SubmitIntakeCommand request, CancellationToken ct)
    {
        // ─────────────────────────────────────────────────────────────────
        // Resolve TenantId.
        // Global admins may specify TenantId in the command; regular users
        // always use their own TenantId from the JWT claim.
        // ─────────────────────────────────────────────────────────────────
        var tenantId = _currentUser.IsGlobal
            ? request.TenantId
            : _currentUser.TenantId;

        // Fail loudly if tenantId is still unresolved.
        if (tenantId == Guid.Empty)
            throw new UnauthorizedAccessException(
                "TenantId could not be resolved. Ensure the JWT contains a valid tenantId claim " +
                "or supply TenantId in the command body (Super Admin only).");

        var userId = _currentUser.UserId;
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException("UserId could not be resolved from the token.");

        // ─────────────────────────────────────────────────────────────────
        // Input validation
        // ─────────────────────────────────────────────────────────────────
        if (request.PersonalInfo == null)
            throw new ArgumentException("PersonalInfo is required.");

        if (string.IsNullOrWhiteSpace(request.PersonalInfo.MedicalNumber))
            throw new ArgumentException("MedicalNumber is required.");

        if (!Enum.TryParse<Gender>(request.PersonalInfo.Gender, true, out var gender))
            throw new ArgumentException($"Invalid gender value: '{request.PersonalInfo.Gender}'.");

        if (request.VisitInfo == null || request.VisitInfo.BranchId == Guid.Empty)
            throw new ArgumentException("VisitInfo.BranchId is required.");

        // ─────────────────────────────────────────────────────────────────
        // Begin a single database transaction that covers ALL writes:
        //   Patient upsert, Intake update, Visit creation, RoomAssignment.
        // AssignmentService does NOT call SaveChangesAsync — it only mutates
        // in-memory entity state (room.Assign()) which EF tracks.
        // ─────────────────────────────────────────────────────────────────
        await using var transaction = await _context.BeginTransactionAsync(ct);

        try
        {
            // ─────────────────────────────────────────────────────────────────
            // Patient — upsert by MedicalNumber within tenant
            // ─────────────────────────────────────────────────────────────────
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p =>
                    p.MedicalNumber == request.PersonalInfo.MedicalNumber.Trim() &&
                    p.TenantId == tenantId, ct);

            if (patient == null)
            {
                patient = new Patient
                {
                    Id            = Guid.NewGuid(),
                    FullName      = request.PersonalInfo.FullName.Trim(),
                    MedicalNumber = request.PersonalInfo.MedicalNumber.Trim(),
                    PhoneNumber   = request.PersonalInfo.Phone,
                    Email         = request.PersonalInfo.Email,
                    DateOfBirth   = request.PersonalInfo.DateOfBirth,
                    Gender        = gender,
                    TenantId      = tenantId,
                    CreatedAt     = DateTime.UtcNow,
                    CreatedBy     = userId
                };

                await _context.Patients.AddAsync(patient, ct);
            }
            else
            {
                // Keep existing patient data; optionally update mutable fields.
                patient.FullName    = request.PersonalInfo.FullName.Trim();
                patient.PhoneNumber = request.PersonalInfo.Phone;
                patient.Email       = request.PersonalInfo.Email;
                patient.UpdatedAt   = DateTime.UtcNow;
                patient.UpdatedBy   = userId;
            }

            // ─────────────────────────────────────────────────────────────────
            // Load Intake — enforce tenant isolation
            // ─────────────────────────────────────────────────────────────────
            var intakeQuery = _context.Intakes
                .Where(x => x.Id == request.IntakeId);

            // Non-global users must only see their own tenant's intakes.
            // Global admins may access any tenant's intake (cross-tenant support).
            if (!_currentUser.IsGlobal)
                intakeQuery = intakeQuery.Where(x => x.TenantId == tenantId);

            var intake = await intakeQuery.FirstOrDefaultAsync(ct);

            if (intake == null)
                throw new InvalidOperationException(
                    $"Intake {request.IntakeId} not found (or not accessible by this tenant).");

            if (intake.Status != IntakeStatus.Draft)
                throw new InvalidOperationException(
                    $"Intake {request.IntakeId} cannot be submitted — current status is '{intake.Status}'.");

            // ─────────────────────────────────────────────────────────────────
            // FIX: Update Intake with request data BEFORE calling AssignAsync.
            //
            // BUG (before fix): AssignmentService reads intake.VisitType and
            // intake.BranchId to determine which room type to find. Without
            // setting these first, the intake still has its Draft defaults
            // (e.g. VisitType = 0 = Outpatient even if user chose Emergency),
            // causing wrong room type selection.
            // ─────────────────────────────────────────────────────────────────
            intake.PatientId = patient.Id;
            intake.BranchId  = request.VisitInfo.BranchId;
            intake.VisitType = request.VisitInfo.VisitType;
            intake.TenantId  = tenantId; // Ensure TenantId is always set

            if (!string.IsNullOrWhiteSpace(request.VisitInfo.Priority) &&
                Enum.TryParse<PriorityLevel>(request.VisitInfo.Priority, true, out var priority))
            {
                intake.Priority = priority;
            }

            if (!string.IsNullOrWhiteSpace(request.VisitInfo.ChiefComplaint))
                intake.ChiefComplaint = request.VisitInfo.ChiefComplaint.Trim();

            if (!string.IsNullOrWhiteSpace(request.VisitInfo.Notes))
                intake.Notes = request.VisitInfo.Notes.Trim();

            if (!string.IsNullOrWhiteSpace(request.VisitInfo.ArrivalMethod) &&
                Enum.TryParse<ArrivalMethod>(request.VisitInfo.ArrivalMethod, true, out var arrivalMethod))
            {
                intake.ArrivalMethod = arrivalMethod;
            }

            // ─────────────────────────────────────────────────────────────────
            // Assign Room + Doctor.
            // AssignmentService only marks room.Assign() in memory — no DB call.
            // ─────────────────────────────────────────────────────────────────
            var (roomId, doctorId) = await _assignment.AssignAsync(
                intake,
                request.VisitInfo.RoomId,
                request.VisitInfo.DoctorId,
                ct);

            // ─────────────────────────────────────────────────────────────────
            // Create Visit
            // ─────────────────────────────────────────────────────────────────
            var visit = new Visit
            {
                Id             = Guid.NewGuid(),
                PatientId      = patient.Id,
                BranchId       = intake.BranchId,
                VisitType      = intake.VisitType,
                Priority       = intake.Priority,
                ArrivalMethod  = intake.ArrivalMethod,
                ChiefComplaint = intake.ChiefComplaint,
                Notes          = intake.Notes,
                DoctorId       = doctorId,
                TenantId       = tenantId,
                PayerType      = Enum.TryParse<PayerType>(request.Payment.PaymentType, true, out var payerType) ? payerType : PayerType.Cash,
                QueueNumber    = await GenerateQueueNumberAsync(intake.BranchId, tenantId, ct),
                CreatedAt      = DateTime.UtcNow,
                CreatedBy      = userId
            };

            visit.SetVisitDate(DateTime.UtcNow);

            await _context.Visits.AddAsync(visit, ct);

            // ─────────────────────────────────────────────────────────────────
            // Create RoomAssignment (only if a room was assigned)
            // ─────────────────────────────────────────────────────────────────
            if (roomId.HasValue)
            {
                await _context.RoomAssignments.AddAsync(new RoomAssignment
                {
                    Id         = Guid.NewGuid(),
                    VisitId    = visit.Id,
                    RoomId     = roomId.Value,
                    IsActive   = true,
                    AssignedAt = DateTime.UtcNow,
                    TenantId   = tenantId,
                    CreatedAt  = DateTime.UtcNow,
                    CreatedBy  = userId
                }, ct);
            }

            // ─────────────────────────────────────────────────────────────────
            // Update Intake status
            // ─────────────────────────────────────────────────────────────────
            intake.Status    = IntakeStatus.ConvertedToVisit;
            intake.UpdatedAt = DateTime.UtcNow;
            intake.UpdatedBy = userId;

            // ─────────────────────────────────────────────────────────────────
            // Single SaveChanges — commits:
            //   • Patient (new or updated)
            //   • Room.IsOccupied = true  (tracked by EF from room.Assign())
            //   • Visit
            //   • RoomAssignment
            //   • Intake status update
            // ─────────────────────────────────────────────────────────────────
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            // ─────────────────────────────────────────────────────────────────
            // Post-commit: real-time notifications (non-critical — failures
            // are logged but do not roll back the committed transaction)
            // ─────────────────────────────────────────────────────────────────
            try
            {
                await _notifier.NotifyNewVisit(tenantId, intake.BranchId);
                if (doctorId.HasValue)
                {
                    await _notifier.NotifyDoctorQueue(doctorId.Value);
                }
                if (roomId.HasValue)
                {
                    await _notifier.NotifyRoomAssigned(tenantId, intake.BranchId);
                }
            }
            catch
            {
                // Swallow notification errors — data is already committed.
                // Use structured logging (ILogger) here in production.
            }

            // ─────────────────────────────────────────────────────────────────
            // Load room number for wristband.
            // ─────────────────────────────────────────────────────────────────
            string? roomNumber = null;
            if (roomId.HasValue)
            {
                roomNumber = await _context.Rooms
                    .IgnoreQueryFilters()
                    .Where(r => r.Id == roomId.Value && r.TenantId == tenantId)
                    .Select(r => r.RoomNumber)
                    .FirstOrDefaultAsync(ct);
            }

            // ─────────────────────────────────────────────────────────────────
            // QR Code
            // ─────────────────────────────────────────────────────────────────
            var qrBytes = _qr.Generate($"{patient.MedicalNumber}|{visit.Id}");

            return new WristbandDto
            {
                PatientName   = patient.FullName,
                MedicalNumber = patient.MedicalNumber,
                RoomNumber    = roomNumber ?? "-",
                QrCode        = qrBytes
            };
        }
        catch
        {
            // Transaction is automatically rolled back when disposed without commit.
            throw;
        }
    }

    /// <summary>
    /// Returns the next queue number for today's visits at a given branch.
    /// Scoped per branch + date to avoid global numbering collisions.
    /// </summary>
    private async Task<int> GenerateQueueNumberAsync(Guid branchId, Guid tenantId, CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;

        var lastQueue = await _context.Visits
            .IgnoreQueryFilters()
            .Where(v =>
                v.TenantId == tenantId &&
                v.BranchId == branchId &&
                v.VisitDate >= today   &&
                v.IsDeleted == false)
            .OrderByDescending(v => v.QueueNumber)
            .Select(v => (int?)v.QueueNumber)
            .FirstOrDefaultAsync(ct);

        return (lastQueue ?? 0) + 1;
    }
}