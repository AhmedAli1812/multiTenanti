using MediatR;

public class CreateIntakeCommand : IRequest<Guid>
{
<<<<<<< HEAD
    public Guid BranchId { get; set; }
=======
    public Guid? BranchId { get; set; }
>>>>>>> origin/main
}