using HMS.SharedKernel.Primitives;

namespace HMS.Intake.Domain.Entities;


// ── Enums ─────────────────────────────────────────────────────────────────────
public enum VisitType     { Outpatient = 1, Emergency = 2, Inpatient = 3 }
public enum ArrivalMethod { WalkIn, Ambulance, Referral, Transfer }
public enum PriorityLevel { Normal, Urgent, Critical }
public enum PaymentType   { Cash, Insurance, Corporate, Charity }
public enum IntakeStatus  { Draft, Submitted, ConvertedToVisit, Cancelled }
public enum Gender        { Male, Female, Other }


// ── Value Objects (replaces JSON blobs — Option B: normalized tables) ─────────

/// <summary>
/// Emergency contact — previously stored as EmergencyContactJson string.
/// Now a proper owned entity with its own table row.
/// </summary>
public sealed class EmergencyContact
{
    private EmergencyContact() { } // EF

    public EmergencyContact(string name, string phone, string? relationship = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Emergency contact name is required.");

        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Emergency contact phone is required.");

        Name         = name.Trim();
        Phone        = phone.Trim();
        Relationship = relationship?.Trim();
    }

    public Guid    IntakeId     { get; private set; }  // FK
    public string  Name         { get; private set; } = default!;
    public string  Phone        { get; private set; } = default!;
    public string? Relationship { get; private set; }
}

/// <summary>
/// Insurance info — previously stored as InsuranceJson string.
/// </summary>
public sealed class InsuranceInfo
{
    private InsuranceInfo() { } // EF

    public InsuranceInfo(string provider, string policyNumber, string? coverageType = null)
    {
        if (string.IsNullOrWhiteSpace(provider))
            throw new DomainException("Insurance provider is required.");

        if (string.IsNullOrWhiteSpace(policyNumber))
            throw new DomainException("Policy number is required.");

        Provider      = provider.Trim();
        PolicyNumber  = policyNumber.Trim();
        CoverageType  = coverageType?.Trim();
    }

    public Guid    IntakeId      { get; private set; }
    public string  Provider      { get; private set; } = default!;
    public string  PolicyNumber  { get; private set; } = default!;
    public string? CoverageType  { get; private set; }
}

/// <summary>
/// Clinical flags — previously stored as FlagsJson string.
/// </summary>
public sealed class IntakeFlags
{
    private IntakeFlags() { } // EF

    public IntakeFlags(bool behaviorAlert, bool fallRisk, bool dnr, bool isolation)
    {
        BehaviorAlert = behaviorAlert;
        FallRisk      = fallRisk;
        Dnr           = dnr;
        Isolation     = isolation;
    }

    public Guid IntakeId      { get; private set; }
    public bool BehaviorAlert { get; private set; }
    public bool FallRisk      { get; private set; }
    public bool Dnr           { get; private set; }
    public bool Isolation     { get; private set; }
}
