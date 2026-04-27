using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Services;
using HMS.Application.Dtos;
using HMS.Application.Features.PatientIntake.Commands.SubmitIntake;
using HMS.Application.Features.Reception.Intake.Commands;
using HMS.Domain.Entities.Operations;
using HMS.Domain.Entities.Patients;
using HMS.Domain.Entities.Visits;
using HMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.PatientIntake.Services;

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
        _context = context;
        _assignment = assignment;
        _qr = qr;
        _notifier = notifier;
        _currentUser = currentUser;
    }

    public async Task<WristbandDto> Handle(SubmitIntakeCommand request, CancellationToken ct)
    {
        var tenantId = _currentUser.IsGlobal
            ? request.TenantId
            : _currentUser.TenantId;

        var userId = _currentUser.UserId;

        // =============================
        // 💣 Validation
        // =============================
        if (request.PersonalInfo == null)
            throw new ArgumentException("Personal info is required");

        if (string.IsNullOrWhiteSpace(request.PersonalInfo.MedicalNumber))
            throw new ArgumentException("Medical number is required");

        if (!Enum.TryParse<Gender>(request.PersonalInfo.Gender, true, out var gender))
            throw new ArgumentException("Invalid gender");

        // =============================
        // 🔥 Transaction
        // =============================
        using var transaction = await _context.BeginTransactionAsync(ct);

        // =============================
        // 🔥 Patient (Upsert)
        // =============================
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p =>
                p.MedicalNumber == request.PersonalInfo.MedicalNumber &&
                p.TenantId == tenantId,
                ct);

        if (patient == null)
        {
            patient = new Patient
            {
                Id = Guid.NewGuid(),
                FullName = request.PersonalInfo.FullName.Trim(),
                MedicalNumber = request.PersonalInfo.MedicalNumber.Trim(),
                PhoneNumber = request.PersonalInfo.Phone,
                Email = request.PersonalInfo.Email,
                DateOfBirth = request.PersonalInfo.DateOfBirth,
                Gender = gender,
                TenantId = tenantId,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _context.Patients.AddAsync(patient, ct);
        }

        // =============================
        // 🔥 Get Intake
        // =============================
        var intakeQuery = _context.Intakes
            .Where(x => x.Id == request.IntakeId);

        if (!_currentUser.IsGlobal)
            intakeQuery = intakeQuery.Where(x => x.TenantId == tenantId);

        var intake = await intakeQuery.FirstOrDefaultAsync(ct);

        if (intake == null)
            throw new InvalidOperationException("Intake not found");

        if (intake.Status != IntakeStatus.Draft)
            throw new InvalidOperationException("Already submitted");

        intake.PatientId = patient.Id;

        // =============================
        // 💣 Assignment
        // =============================
        Guid? doctorId = null;
        Guid? roomId = null;

        if (intake.VisitType == VisitType.Inpatient)
        {
            var result = await _assignment.AssignAsync(intake, ct);

            doctorId = result.doctorId;
            roomId = result.roomId;

            if (doctorId == null)
                throw new Exception("Doctor assignment failed");

            if (roomId == null)
                throw new Exception("Room assignment failed");
        }

        // =============================
        // 🔥 Create Visit
        // =============================
        var visit = new Visit
        {
            Id = Guid.NewGuid(),
            PatientId = patient.Id,
            BranchId = intake.BranchId,
            VisitType = intake.VisitType,
            DoctorId = doctorId,
            TenantId = tenantId,

            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        visit.SetVisitDate(DateTime.UtcNow);

        await _context.Visits.AddAsync(visit, ct);

        // =============================
        // 🛏️ Room Assignment
        // =============================
        if (roomId.HasValue)
        {
            await _context.RoomAssignments.AddAsync(new RoomAssignment
            {
                Id = Guid.NewGuid(),
                VisitId = visit.Id,
                RoomId = roomId.Value,
                IsActive = true,
                TenantId = tenantId,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            }, ct);
        }

        // =============================
        // 🔄 Update Intake
        // =============================
        intake.Status = IntakeStatus.ConvertedToVisit;
        intake.UpdatedAt = DateTime.UtcNow;
        intake.UpdatedBy = userId;

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        // =============================
        // 🔴 Notifications
        // =============================
        await _notifier.NotifyNewVisit(tenantId, intake.BranchId);

        if (doctorId.HasValue)
            await _notifier.NotifyDoctorQueue(doctorId.Value);

        if (roomId.HasValue)
            await _notifier.NotifyRoomAssigned(tenantId, intake.BranchId);

        // =============================
        // 🛏️ Get Room Number
        // =============================
        string? roomNumber = null;

        if (roomId.HasValue)
        {
            roomNumber = await _context.Rooms
                .Where(r => r.Id == roomId.Value)
                .Select(r => r.RoomNumber)
                .FirstOrDefaultAsync(ct);
        }

        // =============================
        // 💣 QR Code
        // =============================
        var qrBytes = _qr.Generate($"{patient.MedicalNumber}|{visit.Id}");

        // =============================
        // 🎯 Return Wristband
        // =============================
        return new WristbandDto
        {
            PatientName = patient.FullName,
            MedicalNumber = patient.MedicalNumber,
            //RoomNumber = roomNumber ?? "-",
            QrCode = qrBytes
        };
    }
}