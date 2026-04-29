using HMS.SharedKernel.Primitives;

namespace HMS.Rooms.Domain.Entities;

public sealed record RoomAssignedEvent(Guid RoomId, Guid TenantId, Guid BranchId) : IDomainEvent
{
    public Guid     Id         { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record RoomReleasedEvent(Guid RoomId, Guid TenantId, Guid BranchId) : IDomainEvent
{
    public Guid     Id         { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
