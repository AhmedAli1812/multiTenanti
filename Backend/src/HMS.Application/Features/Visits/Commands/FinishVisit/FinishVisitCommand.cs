using MediatR;

public class FinishVisitCommand : IRequest<Unit>
{
    public Guid VisitId { get; set; }
}