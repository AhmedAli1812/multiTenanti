using HMS.SharedKernel.Primitives;

namespace HMS.Rooms.Domain.Events;

/// <summary>Raised when a room is assigned to a visit.</summary>
public sealed record RoomAssignedEvent(
    Guid RoomId,
    Guid VisitId,
    Guid TenantId) : IDomainEvent
{
    public Guid     Id         { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
