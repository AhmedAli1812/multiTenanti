using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Application.Dtos;
using HMS.Application.Features.Rooms.GetRooms;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetRoomsHandler : IRequestHandler<GetRoomsQuery, List<RoomDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;

    public GetRoomsHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<RoomDto>> Handle(
    GetRoomsQuery request,
    CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // =========================
        // 🧠 Base Query
        // =========================
        var query = _context.Rooms.AsNoTracking();

        // 🔥 SaaS filter
        Guid? tenantId = null;
        if (!_currentUser.IsGlobal)
        {
            tenantId = _currentUser.TenantId;
        }
        else if (request.TenantId.HasValue)
        {
            tenantId = request.TenantId.Value;
        }

        if (tenantId.HasValue)
        {
            query = query.Where(r => r.TenantId == tenantId.Value);
        }

        // =========================
        // 🔍 Filters
        // =========================
        if (request.BranchId.HasValue)
        {
            query = query.Where(r => r.BranchId == request.BranchId.Value);
        }

        if (request.IsAvailable == true)
        {
            query = query.Where(r =>
                !r.IsOccupied &&
                (r.CleaningUntil == null || r.CleaningUntil <= now));
        }

        // =========================
        // 📄 Data
        // =========================
        return await query
            .OrderBy(r => r.RoomNumber)
            .Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                Capacity = r.Capacity,
                IsOccupied = r.IsOccupied,
                FloorName = r.Floor.Name,
                BranchName = r.Floor.Branch.Name
            })
            .ToListAsync(cancellationToken);
    }
}