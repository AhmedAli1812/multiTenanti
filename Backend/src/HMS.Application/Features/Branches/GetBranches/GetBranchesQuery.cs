using MediatR;

namespace HMS.Application.Features.Branches.GetBranches;

public class GetBranchesQuery : IRequest<List<BranchDto>>
{
    public Guid? TenantId { get; set; }
}