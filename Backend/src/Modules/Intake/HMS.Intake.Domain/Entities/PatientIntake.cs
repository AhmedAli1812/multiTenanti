using HMS.SharedKernel.Primitives;
using HMS.Intake.Domain.Events;

namespace HMS.Intake.Domain.Entities;


/// <summary>
/// PatientIntake aggregate root.
///
/// DDD changes from legacy:
///  - JSON blobs replaced with proper owned value objects (Option B)
///  - State machine enforced via IntakeStatus domain method transitions
///  - Factory method for Draft creation
///  - Submit() domain method raises IntakeSubmittedEvent → decouples from Visits
/// </summary>
public sealed class PatientIntake : TenantEntity, IAggregateRoot
{
    private PatientIntake() { } // EF

    public static PatientIntake CreateDraft(
        Guid         tenantId,
        Guid         branchId,
        VisitType    visitType,
        ArrivalMethod arrivalMethod,
        PriorityLevel priority,
        PaymentType  paymentType,
        string       chiefComplaint,
        string?      notes = null)
    {
        var intake = new PatientIntake
        {
            Id             = Guid.NewGuid(),
            TenantId       = tenantId,
            BranchId       = branchId,
            VisitType      = visitType,
            ArrivalMethod  = arrivalMethod,
            Priority       = priority,
            PaymentType    = paymentType,
            ChiefComplaint = chiefComplaint.Trim(),
            Notes          = notes,
            Status         = IntakeStatus.Draft,
            CreatedAt      = DateTime.UtcNow,
        };

        intake.RaiseDomainEvent(new IntakeDraftCreatedEvent(intake.Id, tenantId));
        return intake;
    }

    // ── Core ───────────────────────────────────────────────────────────────────
    public Guid?        PatientId      { get; private set; }
    public Guid         BranchId       { get; private set; }
    public VisitType    VisitType      { get; private set; }
    public ArrivalMethod ArrivalMethod { get; private set; }
    public PriorityLevel Priority      { get; private set; }
    public PaymentType   PaymentType   { get; private set; }
    public string        ChiefComplaint { get; private set; } = string.Empty;
    public string?       Notes          { get; private set; }
    public IntakeStatus  Status         { get; private set; } = IntakeStatus.Draft;

    // ── Normalized owned entities (replaces JSON blobs) ────────────────────────
    public EmergencyContact? EmergencyContact { get; private set; }
    public InsuranceInfo?    Insurance        { get; private set; }
    public IntakeFlags?      Flags            { get; private set; }

    // ── Domain methods ─────────────────────────────────────────────────────────
    public void LinkPatient(Guid patientId)
    {
        PatientId = patientId;
    }

    public void UpdateVisitInfo(
        Guid          branchId,
        VisitType     visitType,
        PriorityLevel priority,
        ArrivalMethod arrivalMethod,
        string        chiefComplaint,
        string?       notes = null)
    {
        EnsureNotSubmitted();
        BranchId       = branchId;
        VisitType      = visitType;
        Priority       = priority;
        ArrivalMethod  = arrivalMethod;
        ChiefComplaint = chiefComplaint.Trim();
        Notes          = notes;
    }

    public void SetEmergencyContact(string name, string phone, string? relationship = null)
    {
        EnsureNotSubmitted();
        EmergencyContact = new EmergencyContact(name, phone, relationship);
    }

    public void SetInsurance(string provider, string policyNumber, string? coverageType = null)
    {
        EnsureNotSubmitted();
        Insurance = new InsuranceInfo(provider, policyNumber, coverageType);
    }

    public void SetFlags(bool behaviorAlert, bool fallRisk, bool dnr, bool isolation)
    {
        EnsureNotSubmitted();
        Flags = new IntakeFlags(behaviorAlert, fallRisk, dnr, isolation);
    }

    public void Submit(Guid patientId)
    {
        if (Status != IntakeStatus.Draft)
            throw new ConflictException(
                $"Intake '{Id}' cannot be submitted — current status is '{Status}'.");

        if (BranchId == Guid.Empty)
            throw new DomainException("BranchId is required before submission.");

        PatientId = patientId;
        Status    = IntakeStatus.Submitted;

        RaiseDomainEvent(new IntakeSubmittedEvent(
            Id, patientId, TenantId, BranchId, VisitType, Priority, ArrivalMethod, ChiefComplaint, Notes));
    }

    public void MarkConvertedToVisit(Guid visitId)
    {
        if (Status != IntakeStatus.Submitted)
            throw new ConflictException("Can only convert a submitted intake.");

        Status = IntakeStatus.ConvertedToVisit;
        RaiseDomainEvent(new IntakeConvertedEvent(Id, visitId, TenantId));
    }

    private void EnsureNotSubmitted()
    {
        if (Status != IntakeStatus.Draft)
            throw new ConflictException("Cannot modify an intake that is not in Draft status.");
    }
}
