using HMS.Application.Abstractions.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Visits.Commands.DeleteVisit;

public class DeleteVisitCommand : IRequest
{
    public Guid VisitId { get; set; }
}

public class DeleteVisitHandler : IRequestHandler<DeleteVisitCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteVisitHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteVisitCommand request, CancellationToken ct)
    {
        var visit = await _context.Visits
            .Include(v => v.RoomAssignments)
            .FirstOrDefaultAsync(v => v.Id == request.VisitId, ct);

        if (visit == null) return;

        // 1. Remove Room Assignments
        _context.RoomAssignments.RemoveRange(visit.RoomAssignments);

        // 2. Remove the Visit
        _context.Visits.Remove(visit);

        await _context.SaveChangesAsync(ct);
    }
}
