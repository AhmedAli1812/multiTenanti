using MediatR;

namespace HMS.Application.Features.Floors.CreateFloor;

public class CreateFloorCommand : IRequest<Guid>
{
    public string Name { get; set; } = default!;
    public int Number { get; set; }
    public Guid BranchId { get; set; }
}