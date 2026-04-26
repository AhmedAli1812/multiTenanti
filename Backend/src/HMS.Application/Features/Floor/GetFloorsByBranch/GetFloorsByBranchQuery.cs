using MediatR;
using HMS.Application.Dtos;

namespace HMS.Application.Features.Floors.GetFloorsByBranch;

public class GetFloorsByBranchQuery : IRequest<List<FloorDto>>
{
    public Guid BranchId { get; set; }
}