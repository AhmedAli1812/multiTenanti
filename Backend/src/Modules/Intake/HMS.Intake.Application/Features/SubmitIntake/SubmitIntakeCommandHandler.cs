using HMS.Intake.Application.Abstractions;
using HMS.Intake.Domain.Entities;
using HMS.Patients.Domain.Entities;
using HMS.SharedKernel.Application.Abstractions;
using HMS.SharedKernel.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace HMS.Intake.Application.Features.SubmitIntake;

// ── Command ────────────────────────────────────────────────────────────────────
public sealed record SubmitIntakeCommand(
    Guid              IntakeId,
    PersonalInfoDto   PersonalInfo,
    VisitInfoDto      VisitInfo,
    Guid?             TenantId = null   // Super Admin override only
) : ICommand<SubmitIntakeResponse>;

public sealed record PersonalInfoDto(
    string   FullName,
    string   MedicalNumber,
    string   Phone,
    string   Gender,
    DateTime DateOfBirth,
    string?  Email     = null,
    string?  NationalId = null);

public sealed record VisitInfoDto(
    Guid      BranchId,
    VisitType VisitType,
    string    Priority,
    string    ArrivalMethod,
    string    ChiefComplaint);

public sealed record SubmitIntakeResponse(
    Guid   IntakeId,
    Guid   PatientId,
    string PatientName,
    string MedicalNumber,
    int    QueueNumber);

// ── Handler ────────────────────────────────────────────────────────────────────
public sealed class SubmitIntakeCommandHandler(
    IIntakeDbContext context,
    ICurrentUser     currentUser)
    : ICommandHandler<SubmitIntakeCommand, SubmitIntakeResponse>
{
    public async Task<SubmitIntakeResponse> Handle(
        SubmitIntakeCommand request,
        CancellationToken   ct)
    {
        var tenantId = currentUser.TenantId
            ?? (currentUser.IsGlobal ? request.TenantId : null)
            ?? throw new HMS.SharedKernel.Primitives.UnauthorizedException(
                "TenantId could not be resolved.");

        var userId   = currentUser.UserId
            ?? throw new HMS.SharedKernel.Primitives.UnauthorizedException(
                "UserId could not be resolved.");

        // ── Input validation ───────────────────────────────────────────────────
        if (!Enum.TryParse<HMS.Intake.Domain.Entities.Gender>(
                request.PersonalInfo.Gender, true, out var gender))
            throw new HMS.SharedKernel.Primitives.DomainException(
                $"Invalid gender: '{request.PersonalInfo.Gender}'.");

        if (!Enum.TryParse<PriorityLevel>(request.VisitInfo.Priority, true, out var priority))
            throw new HMS.SharedKernel.Primitives.DomainException(
                $"Invalid priority: '{request.VisitInfo.Priority}'.");

        if (!Enum.TryParse<ArrivalMethod>(request.VisitInfo.ArrivalMethod, true, out var arrival))
            throw new HMS.SharedKernel.Primitives.DomainException(
                $"Invalid arrival method: '{request.VisitInfo.ArrivalMethod}'.");

        // ── Patient upsert ─────────────────────────────────────────────────────
        var medNumber = request.PersonalInfo.MedicalNumber.Trim().ToUpperInvariant();

        var patient = await context.Patients
            .FirstOrDefaultAsync(p =>
                p.MedicalNumber == medNumber && p.TenantId == tenantId, ct);

        if (patient is null)
        {
            patient = Patient.Register(
                fullName:     request.PersonalInfo.FullName,
                medicalNumber: medNumber,
                phoneNumber:  request.PersonalInfo.Phone,
                dateOfBirth:  request.PersonalInfo.DateOfBirth,
                gender:       request.PersonalInfo.Gender,
                tenantId:     tenantId,
                email:        request.PersonalInfo.Email,
                nationalId:   request.PersonalInfo.NationalId);

            await context.Patients.AddAsync(patient, ct);
        }
        else
        {
            patient.UpdateContactInfo(
                request.PersonalInfo.Phone,
                request.PersonalInfo.Email,
                address: null);
        }

        // ── Load and validate Intake ───────────────────────────────────────────
        var intake = await context.Intakes
            .FirstOrDefaultAsync(i =>
                i.Id == request.IntakeId && i.TenantId == tenantId, ct)
            ?? throw new HMS.SharedKernel.Primitives.NotFoundException(
                nameof(PatientIntake), request.IntakeId);

        // ── Update intake visit info before Submit() ───────────────────────────
        intake.UpdateVisitInfo(
            branchId:      request.VisitInfo.BranchId,
            visitType:     request.VisitInfo.VisitType,
            priority:      priority,
            arrivalMethod: arrival,
            chiefComplaint: request.VisitInfo.ChiefComplaint);

        // ── Submit — raises IntakeSubmittedEvent → Visits module handles it ────
        intake.Submit(patient.Id);

        // ── Persist ───────────────────────────────────────────────────────────
        // IntakeSubmittedEvent is dispatched AFTER commit by AuditAndDomainEventInterceptor.
        // Visits module's IntakeSubmittedEventHandler then creates Visit + assigns Room.
        await context.SaveChangesAsync(ct);

        return new SubmitIntakeResponse(
            IntakeId:      intake.Id,
            PatientId:     patient.Id,
            PatientName:   patient.FullName,
            MedicalNumber: patient.MedicalNumber,
            QueueNumber:   0);
    }
}
