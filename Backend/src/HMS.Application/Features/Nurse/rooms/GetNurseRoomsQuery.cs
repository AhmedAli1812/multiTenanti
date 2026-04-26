using MediatR;

public class GetNurseRoomsQuery : IRequest<List<NurseRoomDto>>
{
    public Guid BranchId { get; set; }
}