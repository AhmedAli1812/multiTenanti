using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetRoomsDashboardHandler
    : IRequestHandler<GetRoomsDashboardQuery, List<RoomDashboardDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetRoomsDashboardHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<RoomDashboardDto>> Handle(
        GetRoomsDashboardQuery request,
        CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;

        var rooms = await (
            from r in _context.Rooms.AsNoTracking()

            join a in _context.RoomAssignments
                .Where(a => a.IsActive && a.TenantId == tenantId)
                on r.Id equals a.RoomId into ra

            from a in ra.DefaultIfEmpty()

            where r.TenantId == tenantId

            select new RoomDashboardDto
            {
                RoomId = r.Id,
                RoomNumber = r.RoomNumber,

                // 💣 أسرع Check بدون Any()
                IsOccupied = a != null
            }
        ).ToListAsync(ct);

        return rooms;
    }
}