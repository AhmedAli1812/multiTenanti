using HMS.SharedKernel.Primitives;

namespace HMS.Rooms.Domain.Entities;

/// <summary>
/// RoomAssignment — child entity of Room aggregate.
/// Tracks which visit is assigned to which room, with an explicit lifecycle.
/// </summary>
public sealed class RoomAssignment : TenantEntity
{
    private RoomAssignment() { } // EF

    public static RoomAssignment Create(
        Guid visitId,
        Guid roomId,
        Guid tenantId,
        Guid createdBy)
        => new()
        {
            Id         = Guid.NewGuid(),
            VisitId    = visitId,
            RoomId     = roomId,
            IsActive   = true,
            AssignedAt = DateTime.UtcNow,
            TenantId   = tenantId,
            CreatedAt  = DateTime.UtcNow,
            CreatedBy  = createdBy,
        };

    public Guid      RoomId       { get; private set; }
    public Guid      VisitId      { get; private set; }
    public bool      IsActive     { get; private set; }
    public DateTime  AssignedAt   { get; private set; }
    public DateTime? ReleasedAt   { get; private set; }   // FIX: tracks when released

    // Navigation
    public Room  Room  { get; private set; } = default!;

    // ── Domain methods ─────────────────────────────────────────────────────────
    public void Release()
    {
        if (!IsActive)
            throw new DomainException("Assignment is already inactive.");

        IsActive   = false;
        ReleasedAt = DateTime.UtcNow;
    }
}
