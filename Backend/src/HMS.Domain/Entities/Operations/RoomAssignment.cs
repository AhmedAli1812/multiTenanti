using HMS.Domain.Entities.Base;
using HMS.Domain.Entities.Rooms;
using HMS.Domain.Entities.Visits;

namespace HMS.Domain.Entities.Operations;

public class RoomAssignment : TenantEntity
{
    public Guid VisitId { get; set; }
    public Visit Visit { get; set; } = default!;

    public Guid RoomId { get; set; }
    public Room Room { get; set; } = default!;

    // ─────────────────────────────────────────────────────────────────
    // AssignedAt: set explicitly by the caller at the moment of
    // assignment — do NOT use object-init default (= DateTime.UtcNow)
    // because that captures the time the CLR object was constructed,
    // not when it was actually assigned in the database.
    // ─────────────────────────────────────────────────────────────────
    public DateTime AssignedAt { get; set; }

    public DateTime? ReleasedAt { get; set; }

    public bool IsActive { get; set; } = true;
}