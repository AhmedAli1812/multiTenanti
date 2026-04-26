using MediatR;

public class CreateIntakeCommand : IRequest<Guid>
{
    public Guid PatientId { get; set; }
    public Guid? BranchId { get; set; }
}