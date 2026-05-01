using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Tenant;
using HMS.Domain.Entities.Rooms;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HMS.Application.Features.Rooms.CreateRoom;

public class CreateRoomHandler : IRequestHandler<CreateRoomCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantProvider _tenant;
    private readonly ILogger<CreateRoomHandler> _logger;

    public CreateRoomHandler(
        IApplicationDbContext context,
        ITenantProvider tenant,
        ILogger<CreateRoomHandler> logger)
    {
        _context = context;
        _tenant = tenant;
        _logger = logger;
    }

    public async Task<Guid> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating room: {@Request}", request);

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
        {
            _logger.LogWarning("Invalid floor for this branch: FloorId={FloorId}, BranchId={BranchId}, TenantId={TenantId}", 
                request.FloorId, request.BranchId, tenantId);
            throw new InvalidOperationException("Invalid floor for this branch");
        }

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
        {
            _logger.LogWarning("Room already exists in this branch: {RoomNumber}", roomNumber);
            throw new InvalidOperationException("Room already exists in this branch");
        }

        // =========================
        // 🧠 CREATE ROOM
        // =========================
        var room = new Room
        {
            Id = Guid.NewGuid(),
            RoomNumber = roomNumber,
            Name = string.IsNullOrWhiteSpace(request.Name) ? $"Room {roomNumber}" : request.Name.Trim(),
            Type = (HMS.Domain.Enums.RoomType)(request.Type ?? 1), // Default to Clinic if null
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