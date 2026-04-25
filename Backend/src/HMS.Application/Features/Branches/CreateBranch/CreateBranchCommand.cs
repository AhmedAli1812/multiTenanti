using MediatR;

namespace HMS.Application.Features.Branches.CreateBranch;

public class CreateBranchCommand : IRequest<Guid>
{
    public string Name { get; set; } = default!;
}