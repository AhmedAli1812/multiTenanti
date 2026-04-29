using HMS.Rooms.Application.Abstractions;
using HMS.Rooms.Domain.Entities;
using HMS.SharedKernel.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HMS.Rooms.Application.Features.AssignRoom;

public sealed record AssignRoomCommand(
    Guid TenantId,
    Guid BranchId,
    RoomType PreferredType
) : ICommand<AssignRoomResponse>;

public sealed record AssignRoomResponse(Guid RoomId, string RoomNumber);

public sealed class AssignRoomCommandHandler(IRoomsDbContext context)
    : ICommandHandler<AssignRoomCommand, AssignRoomResponse>
{
    public async Task<AssignRoomResponse> Handle(
        AssignRoomCommand request,
        CancellationToken ct)
    {
        // Use UPDLOCK via raw SQL to prevent race conditions on concurrent assignments
        var room = await context.Rooms
            .FromSqlRaw(@"
                SELECT TOP 1 r.* FROM rooms.Rooms r WITH (UPDLOCK, ROWLOCK)
                WHERE r.TenantId = {0}
                  AND r.BranchId = {1}
                  AND r.IsOccupied = 0
                  AND r.IsDeleted  = 0
                  AND (r.CleaningUntil IS NULL OR r.CleaningUntil <= GETUTCDATE())
                ORDER BY r.RoomNumber",
                request.TenantId, request.BranchId)
            .FirstOrDefaultAsync(ct)
            ?? throw new HMS.SharedKernel.Primitives.ConflictException(
                $"No available room found in branch '{request.BranchId}'.");

        room.Assign();
        await context.SaveChangesAsync(ct);

        return new AssignRoomResponse(room.Id, room.RoomNumber);
    }
}
