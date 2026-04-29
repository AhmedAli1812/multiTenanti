using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Tenant;
using HMS.Domain.Entities.Rooms;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Rooms.CreateRoom;

public class CreateRoomHandler : IRequestHandler<CreateRoomCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenant;

    public CreateRoomHandler(
        IApplicationDbContext context,
        ITenantProvider tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (request.TenantId.HasValue && _tenant.IsSuperAdmin())
            ? request.TenantId.Value
            : _tenant.GetTenantId();

        if (tenantId == null)
            throw new ArgumentException("Tenant ID is required");

        // =========================
        // 💣 VALIDATION
        // =========================
        if (string.IsNullOrWhiteSpace(request.RoomNumber))
            throw new ArgumentException("Room number is required");

        if (request.Capacity <= 0)
            throw new ArgumentException("Capacity must be greater than 0");

        var roomNumber = request.RoomNumber.Trim();

        // =========================
        // 🔍 Validate Floor + Branch
        // =========================
        var floorExists = await _context.Floors
            .AsNoTracking()
            .AnyAsync(f =>
                f.Id == request.FloorId &&
                f.BranchId == request.BranchId &&
                f.TenantId == tenantId,
                cancellationToken);

        if (!floorExists)
            throw new InvalidOperationException("Invalid floor for this branch");

        // =========================
        // 🚫 Prevent duplicate room
        // =========================
        var exists = await _context.Rooms
            .AsNoTracking()
            .AnyAsync(r =>
                r.RoomNumber == roomNumber &&
                r.BranchId == request.BranchId &&
                r.TenantId == tenantId,
                cancellationToken);

        if (exists)
            throw new InvalidOperationException("Room already exists in this branch");

        // =========================
        // 🧠 CREATE ROOM
        // =========================
        var room = new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = roomNumber,
            Capacity = request.Capacity,
            FloorId = request.FloorId,
            BranchId = request.BranchId,
            TenantId = tenantId
        };

        await _context.Rooms.AddAsync(room, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return room.Id;
    }
}