using HMS.Application.Dtos;
using HMS.Application.Features.Rooms.GetRooms;
using MediatR;

public class GetRoomsQuery : IRequest<List<RoomDto>>
{
    public Guid? BranchId { get; set; }
    public Guid? TenantId { get; set; }
    public bool? IsAvailable { get; set; }
}