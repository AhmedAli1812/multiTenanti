namespace HMS.Application.Features.Rooms.GetRooms;

public class RoomDto
{
    public Guid Id { get; set; }
    public string RoomNumber { get; set; } = default!;
    public int Capacity { get; set; }
    public bool IsOccupied { get; set; }

    public string FloorName { get; set; } = default!;
    public string BranchName { get; set; } = default!;
}