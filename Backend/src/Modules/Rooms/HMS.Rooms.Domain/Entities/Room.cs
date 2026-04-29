using HMS.SharedKernel.Primitives;

namespace HMS.Rooms.Domain.Entities;

/// <summary>
/// Room aggregate root.
///
/// DDD changes:
///  - Concurrency token on IsOccupied to prevent double-booking race conditions
///  - Capacity tracking (CurrentOccupancy counter)
///  - Assignment/Release go through domain methods that raise events
///  - RoomAssignment is a child entity within this aggregate
/// </summary>
public sealed class Room : TenantEntity, IAggregateRoot
{
    private Room() { } // EF

    public static Room Create(
        string   name,
        string   roomNumber,
        RoomType type,
        int      capacity,
        Guid     branchId,
        Guid     floorId,
        Guid     tenantId)
    {
        if (string.IsNullOrWhiteSpace(roomNumber))
            throw new DomainException("Room number is required.");

        if (capacity < 1)
            throw new DomainException("Capacity must be at least 1.");

        return new Room
        {
            Id          = Guid.NewGuid(),
            Name        = name.Trim(),
            RoomNumber  = roomNumber.Trim().ToUpperInvariant(),
            Type        = type,
            Capacity    = capacity,
            BranchId    = branchId,
            FloorId     = floorId,
            TenantId    = tenantId,
            CreatedAt   = DateTime.UtcNow,
        };
    }

    // ── Identity ───────────────────────────────────────────────────────────────
    public string   Name        { get; private set; } = default!;
    public string   RoomNumber  { get; private set; } = default!;
    public RoomType Type        { get; private set; }
    public int      Capacity    { get; private set; } = 1;

    // ── Location ───────────────────────────────────────────────────────────────
    public Guid BranchId { get; private set; }
    public Guid FloorId  { get; private set; }

    // ── Occupancy ─────────────────────────────────────────────────────────────
    public bool      IsOccupied        { get; private set; }
    public int       CurrentOccupancy  { get; private set; }   // tracks multi-bed rooms
    public DateTime? CleaningUntil     { get; private set; }

    /// <summary>
    /// EF Core concurrency token — prevents two simultaneous transactions
    /// from assigning the same room. If two transactions both read IsOccupied=false
    /// and try to update, the second will throw a DbUpdateConcurrencyException.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] RowVersion { get; private set; } = default!;

    // ── Domain logic ───────────────────────────────────────────────────────────
    public bool IsAvailable()
    {
        var now = DateTime.UtcNow;
        return !IsOccupied
            && CurrentOccupancy < Capacity
            && (CleaningUntil == null || CleaningUntil <= now);
    }

    public void Assign()
    {
        if (!IsAvailable())
            throw new ConflictException($"Room '{RoomNumber}' is not available.");

        CurrentOccupancy++;
        if (CurrentOccupancy >= Capacity)
            IsOccupied = true;

        RaiseDomainEvent(new RoomAssignedEvent(Id, TenantId, BranchId));
    }

    public void Release()
    {
        if (CurrentOccupancy > 0)
            CurrentOccupancy--;

        IsOccupied    = CurrentOccupancy >= Capacity;
        CleaningUntil = DateTime.UtcNow.AddMinutes(15);

        RaiseDomainEvent(new RoomReleasedEvent(Id, TenantId, BranchId));
    }

    public string GetStatus()
    {
        var now = DateTime.UtcNow;
        if (CleaningUntil != null && CleaningUntil > now) return "Cleaning";
        if (IsOccupied)                                    return "Occupied";
        return "Available";
    }
}

public enum RoomType { General, ICU, Emergency, Private, Semiprivate, OR }
