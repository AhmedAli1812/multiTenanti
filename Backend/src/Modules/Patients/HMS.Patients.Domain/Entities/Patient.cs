using HMS.SharedKernel.Primitives;

namespace HMS.Patients.Domain.Entities;

/// <summary>
/// Patient aggregate root.
///
/// DDD improvements:
///  - Factory method enforces creation invariants
///  - MedicalNumber is unique per tenant — enforced at DB level + domain check
///  - All mutating operations go through domain methods
///  - Domain events on registration and update
/// </summary>
public sealed class Patient : TenantEntity, IAggregateRoot
{
    private Patient() { } // EF

    public static Patient Register(
        string   fullName,
        string   medicalNumber,
        string   phoneNumber,
        DateTime dateOfBirth,
        string   gender,
        Guid     tenantId,
        string?  email             = null,
        string?  nationalId        = null,
        string?  address           = null,
        string?  emergencyContactName  = null,
        string?  emergencyContactPhone = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Patient full name is required.");

        if (string.IsNullOrWhiteSpace(medicalNumber))
            throw new DomainException("Medical number is required.");

        if (dateOfBirth >= DateTime.UtcNow)
            throw new DomainException("Date of birth must be in the past.");

        var patient = new Patient
        {
            Id                    = Guid.NewGuid(),
            FullName              = fullName.Trim(),
            MedicalNumber         = medicalNumber.Trim().ToUpperInvariant(),
            PhoneNumber           = phoneNumber.Trim(),
            Email                 = email?.Trim().ToLowerInvariant(),
            DateOfBirth           = dateOfBirth,
            Gender                = gender,
            NationalId            = nationalId?.Trim(),
            Address               = address?.Trim(),
            EmergencyContactName  = emergencyContactName?.Trim(),
            EmergencyContactPhone = emergencyContactPhone?.Trim(),
            TenantId              = tenantId,
            CreatedAt             = DateTime.UtcNow,
        };

        patient.RaiseDomainEvent(new PatientRegisteredEvent(patient.Id, tenantId, medicalNumber));
        return patient;
    }

    // ── Identity ───────────────────────────────────────────────────────────────
    public string   FullName              { get; private set; } = default!;
    public string   MedicalNumber         { get; private set; } = default!;  // unique per tenant
    public string   PhoneNumber           { get; private set; } = default!;
    public string?  Email                 { get; private set; }
    public string   Gender                { get; private set; } = default!;
    public DateTime DateOfBirth           { get; private set; }

    // ── Personal ───────────────────────────────────────────────────────────────
    public string? NationalId            { get; private set; }
    public string? Address               { get; private set; }

    // ── Emergency contact ──────────────────────────────────────────────────────
    public string? EmergencyContactName  { get; private set; }
    public string? EmergencyContactPhone { get; private set; }

    // ── Files ──────────────────────────────────────────────────────────────────
    public string? IdCardFrontUrl { get; private set; }
    public string? IdCardBackUrl  { get; private set; }

    // ── Domain methods ─────────────────────────────────────────────────────────
    public void UpdateContactInfo(string phone, string? email, string? address)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Phone number is required.");

        PhoneNumber = phone.Trim();
        Email       = email?.Trim().ToLowerInvariant();
        Address     = address?.Trim();

        RaiseDomainEvent(new PatientUpdatedEvent(Id, TenantId));
    }

    public void UpdateProfile(string fullName, DateTime dob)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");

        if (dob >= DateTime.UtcNow)
            throw new DomainException("Date of birth must be in the past.");

        FullName    = fullName.Trim();
        DateOfBirth = dob;
        RaiseDomainEvent(new PatientUpdatedEvent(Id, TenantId));
    }

    public void SetIdCardImages(string? frontUrl, string? backUrl)
    {
        IdCardFrontUrl = frontUrl;
        IdCardBackUrl  = backUrl;
    }

    public int Age() => (int)((DateTime.UtcNow - DateOfBirth).TotalDays / 365.25);
}
