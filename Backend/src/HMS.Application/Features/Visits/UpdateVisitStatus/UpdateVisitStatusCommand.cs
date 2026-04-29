using MediatR;
using HMS.Domain.Enums;

namespace HMS.Application.Features.Visits.UpdateVisitStatus;

public class UpdateVisitStatusCommand : IRequest<Unit>
{
    public Guid VisitId { get; set; }
    public VisitStatus Status { get; set; }
}