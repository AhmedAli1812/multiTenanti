using MediatR;

namespace HMS.Application.Features.Branches.GetBranches;

public class GetBranchesQuery : IRequest<List<BranchDto>>
{
}