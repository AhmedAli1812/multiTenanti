public class NurseRoomDto
{
    public Guid RoomId { get; set; }
    public string? PatientName { get; set; }

    public string RoomNumber { get; set; } = default!;

    public bool IsOccupied { get; set; }

    public DateTime? CleaningUntil { get; set; }

    public string Status { get; set; } = default!;

    public Guid? CurrentVisitId { get; set; }
}