using HMS.Domain.Entities.Base;
using HMS.Domain.Enums;


namespace HMS.Domain.Entities.PatientIntake;

public class PatientIntake : TenantEntity
{
    public Guid TenantId { get; set; }
    public Guid? PatientId { get; set; }
    public Guid BranchId { get; set; }
    public VisitType VisitType { get; set; }
    public ArrivalMethod ArrivalMethod { get; set; }
    public PriorityLevel Priority { get; set; }

    public string ChiefComplaint { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public PaymentType PaymentType { get; set; }

    public IntakeStatus Status { get; set; } = IntakeStatus.Draft;

    // JSON (temporary)
    public string? EmergencyContactJson { get; set; }
    public string? InsuranceJson { get; set; }
    public string? FlagsJson { get; set; }
}