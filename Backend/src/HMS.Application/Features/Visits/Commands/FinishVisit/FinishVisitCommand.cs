using MediatR;

namespace HMS.Application.Features.Visits.Commands.FinishVisit;

public class FinishVisitCommand : IRequest<Unit>
{
    public Guid VisitId { get; set; }
}