using HMS.Domain.Enums;

public class VisitInfoDto
{
    public Guid BranchId { get; set; }

    public VisitType VisitType { get; set; } // 🔥 بدل string
    public string ArrivalMethod { get; set; } = default!;
    public string Priority { get; set; } = default!;

    public string? ChiefComplaint { get; set; }

    public Guid? DoctorId { get; set; }
    public Guid? RoomId { get; set; }
}