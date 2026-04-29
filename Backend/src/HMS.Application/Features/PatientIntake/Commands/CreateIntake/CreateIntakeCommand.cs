using MediatR;

public class CreateIntakeCommand : IRequest<Guid>
{

    public Guid? BranchId { get; set; }

}