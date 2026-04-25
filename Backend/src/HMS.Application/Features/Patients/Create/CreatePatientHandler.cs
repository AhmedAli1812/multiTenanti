using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Tenant;
using HMS.Application.Abstractions.Services;
using HMS.Domain.Entities.Patients;
using HMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HMS.Application.Common.Exceptions;

namespace HMS.Application.Features.Patients.Create;

public class CreatePatientHandler : IRequestHandler<CreatePatientCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenant;
    private readonly INotificationService _notification;

    public CreatePatientHandler(
        IApplicationDbContext context,
        ITenantProvider tenant,
        INotificationService notification)
    {
        _context = context;
        _tenant = tenant;
        _notification = notification;
    }

    public async Task<Guid> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenant.GetTenantId();

        // =========================
        // 💣 VALIDATION
        // =========================
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new BadRequestException("Full name is required");

        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            throw new BadRequestException("Phone number is required");

        if (request.DateOfBirth == default)
            throw new BadRequestException("Date of birth is required");

        var fullName = request.FullName.Trim();
        var phone = request.PhoneNumber.Trim();
        var email = request.Email?.Trim().ToLower();

        // =========================
        // 🔥 TRANSACTION
        // =========================
        using var transaction = await _context.BeginTransactionAsync(cancellationToken);

        // =========================
        // 🔥 CHECK PHONE DUPLICATE
        // =========================
        var phoneExists = await _context.Patients
            .AsNoTracking()
            .AnyAsync(x => x.PhoneNumber == phone && x.TenantId == tenantId, cancellationToken);

        if (phoneExists)
            throw new BadRequestException("Patient with same phone already exists");

        // =========================
        // 🔥 GENERATE UNIQUE MRN (Safe)
        // =========================
        string medicalNumber;
        int retries = 0;

        do
        {
            medicalNumber = request.MedicalNumber?.Trim();

            if (string.IsNullOrWhiteSpace(medicalNumber))
                medicalNumber = GenerateMedicalNumber();

            var exists = await _context.Patients
                .AsNoTracking()
                .AnyAsync(x => x.MedicalNumber == medicalNumber && x.TenantId == tenantId, cancellationToken);

            if (!exists) break;

            retries++;
        }
        while (retries < 3);

        if (retries == 3)
            throw new BadRequestException("Failed to generate unique medical number");

        // =========================
        // 🧠 CREATE PATIENT
        // =========================
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,

            FullName = fullName,
            MedicalNumber = medicalNumber,

            PhoneNumber = phone,
            NationalId = request.NationalId,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,

            Address = request.Address?.Trim(),
            Email = email,

            EmergencyContactName = request.EmergencyContactName?.Trim(),
            EmergencyContactPhone = request.EmergencyContactPhone?.Trim()
        };

        await _context.Patients.AddAsync(patient, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // =========================
        // 🔥 COMMIT
        // =========================
        await transaction.CommitAsync(cancellationToken);

        // =========================
        // 🔔 NOTIFICATIONS (Safe)
        // =========================
        try
        {
            await _notification.SendToRoleAsync(
                "Nurse",
                "New Patient Added",
                $"Patient {patient.FullName} has been added"
            );

            if (request.DoctorId.HasValue)
            {
                await _notification.SendAsync(new Notification
                {
                    UserId = request.DoctorId.Value,
                    Title = "New Patient Assigned",
                    Message = $"You have a new patient: {patient.FullName}",
                    TenantId = tenantId
                });
            }
        }
        catch
        {
            // 💣 ما نكسرش العملية بسبب notification
        }

        return patient.Id;
    }

    // =========================
    // 🔢 GENERATOR
    // =========================
    private string GenerateMedicalNumber()
    {
        return $"MRN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}