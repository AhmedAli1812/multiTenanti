using HMS.Domain.Entities.Base;
using HMS.Domain.Entities.Rooms;
using HMS.Domain.Entities.Visits;

namespace HMS.Domain.Entities.Operations;
public class RoomAssignment : TenantEntity
{
    public Guid VisitId { get; set; }
    public Visit Visit { get; set; } = default!; // 🔥

    public Guid RoomId { get; set; }
    public Room Room { get; set; } = default!; // 🔥

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReleasedAt { get; set; }

    public bool IsActive { get; set; } = true;
}