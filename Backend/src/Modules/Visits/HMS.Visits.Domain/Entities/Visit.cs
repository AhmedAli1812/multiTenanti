using HMS.SharedKernel.Primitives;
using HMS.Visits.Domain.Events;

namespace HMS.Visits.Domain.Entities;


public enum VisitStatus
{
    CheckedIn, WaitingDoctor, Prepared, InOp, OpCompleted, PostOp, Completed
}

/// <summary>
/// Visit aggregate root.
///
/// DDD improvements:
///  - ChangeStatus() enforces state machine
///  - Factory method used by IntakeSubmittedEventHandler
///  - Domain events on creation and completion for real-time notifications
/// </summary>
public sealed class Visit : TenantEntity, IAggregateRoot
{
    private Visit() { } // EF

    public static Visit Create(
        Guid       patientId,
        Guid       branchId,
        VisitType  visitType,
        Guid       tenantId,
        int        queueNumber,
        Guid?      doctorId  = null,
        Guid?      createdBy = null)
    {
        var visit = new Visit
        {
            Id          = Guid.NewGuid(),
            PatientId   = patientId,
            BranchId    = branchId,
            VisitType   = visitType,
            DoctorId    = doctorId,
            TenantId    = tenantId,
            QueueNumber = queueNumber,
            Status      = VisitStatus.CheckedIn,
            VisitDate   = DateTime.UtcNow,
            CreatedAt   = DateTime.UtcNow,
            CreatedBy   = createdBy,
        };

        visit.RaiseDomainEvent(new VisitCreatedEvent(visit.Id, patientId, tenantId, branchId));
        return visit;
    }

    // ── Core ───────────────────────────────────────────────────────────────────
    public Guid       PatientId   { get; private set; }
    public Guid?      DoctorId    { get; private set; }
    public Guid       BranchId    { get; private set; }
    public VisitType  VisitType   { get; private set; }
    public PayerType  PayerType   { get; set; }
    public VisitStatus Status     { get; private set; } = VisitStatus.CheckedIn;
    public int        QueueNumber { get; private set; }

    // ── Tracking ───────────────────────────────────────────────────────────────
    public DateTime  VisitDate   { get; private set; }
    public DateTime? StartedAt   { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    // ── Domain logic ───────────────────────────────────────────────────────────
    public void ChangeStatus(VisitStatus newStatus)
    {
        if (!IsValidTransition(Status, newStatus))
            throw new DomainException(
                $"Invalid visit status transition: {Status} → {newStatus}.",
                "INVALID_STATUS_TRANSITION");

        Status = newStatus;

        switch (newStatus)
        {
            case VisitStatus.InOp:
                StartedAt = DateTime.UtcNow;
                break;

            case VisitStatus.Completed:
                CompletedAt = DateTime.UtcNow;
                RaiseDomainEvent(new VisitCompletedEvent(Id, PatientId, TenantId));
                break;
        }
    }

    public void AssignDoctor(Guid doctorId) => DoctorId = doctorId;

    private static bool IsValidTransition(VisitStatus current, VisitStatus next)
        => current switch
        {
            VisitStatus.CheckedIn     => next == VisitStatus.WaitingDoctor,
            VisitStatus.WaitingDoctor => next == VisitStatus.Prepared,
            VisitStatus.Prepared      => next == VisitStatus.InOp,
            VisitStatus.InOp          => next == VisitStatus.OpCompleted,
            VisitStatus.OpCompleted   => next == VisitStatus.PostOp,
            VisitStatus.PostOp        => next == VisitStatus.Completed,
            _                         => false
        };
}

public enum VisitType  { Outpatient = 1, Emergency = 2, Inpatient = 3 }
public enum PayerType  { Cash, Insurance, Referral }
