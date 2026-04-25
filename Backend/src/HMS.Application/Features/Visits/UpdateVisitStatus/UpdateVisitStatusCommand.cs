using MediatR;
using HMS.Domain.Enums;

public class UpdateVisitStatusCommand : IRequest<Unit>
{
    public Guid VisitId { get; set; }
    public VisitStatus Status { get; set; }
}