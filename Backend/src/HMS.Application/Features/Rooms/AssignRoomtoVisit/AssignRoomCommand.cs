using MediatR;

public class AssignRoomCommand : IRequest
{
    public Guid VisitId { get; set; }
    public Guid RoomId { get; set; }
}