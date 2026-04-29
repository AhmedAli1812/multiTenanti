using HMS.Domain.Entities.Base;
using HMS.Domain.Entities.Branches;
using HMS.Domain.Entities.Identity;
using HMS.Domain.Entities.Patients;
using HMS.Domain.Entities.Rooms;
using HMS.Domain.Enums;
using HMS.Domain.Entities.Operations;

namespace HMS.Domain.Entities.Visits;

public class Visit : TenantEntity
{
    // 🆔
    public Guid Id { get; set; }

    // =============================
    // 🔗 Relations
    // =============================

    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = default!;

    // 💣 OPTIONAL (حسب نوع الزيارة)
    public Guid? DoctorId { get; set; }
    public User? Doctor { get; set; }
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = default!;
    public ICollection<RoomAssignment> RoomAssignments { get; set; } = new List<RoomAssignment>();

    // =============================
    // 🔥 Core Data
    // =============================

    // (Outpatient / ER / Inpatient)
    public VisitType VisitType { get; set; }

    public PriorityLevel Priority { get; set; }

    public ArrivalMethod ArrivalMethod { get; set; }

    public string ChiefComplaint { get; set; } = string.Empty;

    public PayerType PayerType { get; set; }

    public VisitStatus Status { get; private set; } = VisitStatus.CheckedIn;

    public int QueueNumber { get; set; }

    // =============================
    // ⏱️ Tracking
    // =============================

    public DateTime VisitDate { get; private set; } = DateTime.UtcNow;

    public DateTime? StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    // =====================================================
    // 🔁 Domain Logic
    // =====================================================

    public void ChangeStatus(VisitStatus newStatus)
    {
        if (!IsValidTransition(Status, newStatus))
            throw new InvalidOperationException(
                $"Invalid transition from {Status} to {newStatus}");

        Status = newStatus;

        switch (newStatus)
        {
            case VisitStatus.InOp:
                StartedAt = DateTime.UtcNow;
                break;

            case VisitStatus.Completed:
                CompletedAt = DateTime.UtcNow;
                break;
        }
    }

    // 🔒 منع تعديل VisitDate من برا
    public void SetVisitDate(DateTime date)
    {
        VisitDate = date;
    }

    private static bool IsValidTransition(VisitStatus current, VisitStatus next)
    {
        if (next == VisitStatus.Completed) return true;

        return current switch
        {
            VisitStatus.CheckedIn => next == VisitStatus.WaitingDoctor,
            VisitStatus.WaitingDoctor => next == VisitStatus.Prepared,
            VisitStatus.Prepared => next == VisitStatus.InOp,
            VisitStatus.InOp => next == VisitStatus.OpCompleted,
            VisitStatus.OpCompleted => next == VisitStatus.PostOp,
            VisitStatus.PostOp => next == VisitStatus.Completed,
            _ => false
        };
    }
}