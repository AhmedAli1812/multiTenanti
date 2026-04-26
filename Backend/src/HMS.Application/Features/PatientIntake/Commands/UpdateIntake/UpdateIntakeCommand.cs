using MediatR;
using HMS.Domain.Enums;

public class UpdateIntakeCommand : IRequest<Unit> // 🔥 مهم جدًا
{
    public Guid IntakeId { get; set; }

    public Guid? BranchId { get; set; }
    public Guid? RoomId { get; set; }

    public VisitType? VisitType { get; set; }
    public PriorityLevel? Priority { get; set; }

    public string? ChiefComplaint { get; set; }

    public string? EmergencyContactJson { get; set; }
    public string? InsuranceJson { get; set; }
    public string? FlagsJson { get; set; }
}