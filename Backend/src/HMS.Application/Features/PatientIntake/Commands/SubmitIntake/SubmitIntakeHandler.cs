using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Services;
using HMS.Application.Features.PatientIntake.Commands.SubmitIntake;
using HMS.Application.Dtos;
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

    public SubmitIntakeHandler(
        IApplicationDbContext context,
        IAssignmentService assignment,
        IQrCodeService qr,
        IDashboardNotifier notifier)
    {
        _context = context;
        _assignment = assignment;
        _qr = qr;
        _notifier = notifier;
    }

    public async Task<WristbandDto> Handle(SubmitIntakeCommand request, CancellationToken ct)
    {
        // =============================
        // 💣 Validation
        // =============================
        if (request.PersonalInfo == null)
            throw new ArgumentException("Personal info is required");

        if (string.IsNullOrWhiteSpace(request.PersonalInfo.MedicalNumber))
            throw new ArgumentException("Medical number is required");

        if (!Enum.TryParse<Gender>(request.PersonalInfo.Gender, true, out var gender))
            throw new ArgumentException("Invalid gender");

        using var transaction = await _context.BeginTransactionAsync(ct);

        // =============================
        // 🔥 Patient (Upsert)
        // =============================
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p =>
                p.MedicalNumber == request.PersonalInfo.MedicalNumber &&
                p.TenantId == request.TenantId,
                ct);

        if (patient == null)
        {
            patient = new Patient
            {
                FullName = request.PersonalInfo.FullName.Trim(),
                MedicalNumber = request.PersonalInfo.MedicalNumber.Trim(),
                PhoneNumber = request.PersonalInfo.Phone,
                Email = request.PersonalInfo.Email,
                DateOfBirth = request.PersonalInfo.DateOfBirth,
                Gender = gender,
                TenantId = request.TenantId
            };

            await _context.Patients.AddAsync(patient, ct);
        }

        // =============================
        // 🔥 Intake
        // =============================
        var intake = await _context.Intakes
            .FirstOrDefaultAsync(x =>
                x.Id == request.IntakeId &&
                x.TenantId == request.TenantId,
                ct);

        if (intake == null)
            throw new InvalidOperationException("Intake not found");

        if (intake.Status != IntakeStatus.Draft)
            throw new InvalidOperationException("Already submitted");

        intake.PatientId = patient.Id;

        // =============================
        // 💣 Assignment Logic
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
        // 🔥 Create Visit (NO RoomId ❌)
        // =============================
        var visit = new Visit
        {
            PatientId = patient.Id,
            BranchId = intake.BranchId,
            VisitType = intake.VisitType,
            DoctorId = doctorId,
            TenantId = request.TenantId
        };

        visit.SetVisitDate(DateTime.UtcNow);

        await _context.Visits.AddAsync(visit, ct);

        // =============================
        // 🛏️ Room Assignment (ONLY HERE)
        // =============================
        if (roomId.HasValue)
        {
            await _context.RoomAssignments.AddAsync(new RoomAssignment
            {
                VisitId = visit.Id,
                RoomId = roomId.Value,
                IsActive = true,
                TenantId = request.TenantId
            }, ct);
        }

        // =============================
        // 🔄 Update Intake
        // =============================
        intake.Status = IntakeStatus.ConvertedToVisit;

        await _context.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        // =============================
        // 🔴 Real-time (SMART EVENTS)
        // =============================

        // 🟢 Reception → فيه مريض جديد
        await _notifier.NotifyNewVisit(request.TenantId, intake.BranchId);

        // 👨‍⚕️ Doctor → الطابور اتغير
        if (doctorId.HasValue)
        {
            await _notifier.NotifyDoctorQueue(doctorId.Value);
        }

        // 🏥 Nurses → الغرف اتغيرت
        if (roomId.HasValue)
        {
            await _notifier.NotifyRoomAssigned(request.TenantId, intake.BranchId);
        }
        // =============================
        // 💣 QR Code
        // =============================
        var qrBytes = _qr.Generate($"https://your-domain/api/patients/{patient.Id}");

        return new WristbandDto
        {
            PatientName = patient.FullName,
            MedicalNumber = patient.MedicalNumber,
            QrCode = qrBytes
        };
    }
}