using HMS.SharedKernel.Primitives;
using HMS.Intake.Domain.Entities;

namespace HMS.Intake.Domain.Events;


public sealed record IntakeDraftCreatedEvent(Guid IntakeId, Guid TenantId) : IDomainEvent
{
    public Guid     Id         { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// IntakeSubmittedEvent — the key cross-module event.
///
/// When Intake raises this event, the Visits module's IntakeSubmittedEventHandler
/// picks it up and creates the Visit + assigns Room + Doctor.
///
/// This replaces the direct IAssignmentService call that was tightly coupling
/// the Intake and Visits modules.
/// </summary>
public sealed record IntakeSubmittedEvent(
    Guid          IntakeId,
    Guid          PatientId,
    Guid          TenantId,
    Guid          BranchId,
    VisitType     VisitType,
    PriorityLevel Priority) : IDomainEvent
{
    public Guid     Id         { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record IntakeConvertedEvent(
    Guid IntakeId,
    Guid VisitId,
    Guid TenantId) : IDomainEvent
{
    public Guid     Id         { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
