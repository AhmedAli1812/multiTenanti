using MediatR;

namespace HMS.Application.Features.Rooms.CreateRoom;

public class CreateRoomCommand : IRequest<Guid>
{
    public string RoomNumber { get; set; } = default!;
    public string? Name { get; set; }
    public int? Type { get; set; } // RoomType enum
    public int Capacity { get; set; }
    public Guid FloorId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? TenantId { get; set; }
}