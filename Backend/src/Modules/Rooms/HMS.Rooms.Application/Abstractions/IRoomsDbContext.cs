using HMS.Rooms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HMS.Rooms.Application.Abstractions;

/// <summary>Rooms module slice of HmsDbContext.</summary>
public interface IRoomsDbContext
{
    DbSet<Room>           Rooms           { get; }
    DbSet<RoomAssignment> RoomAssignments { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}
