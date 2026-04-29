using HMS.SharedKernel.Primitives;

namespace HMS.Identity.Domain.Entities;

public sealed record UserCreatedEvent(Guid UserId, Guid TenantId) : IDomainEvent
{
    public Guid     Id         { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record UserLockedEvent(Guid UserId, Guid TenantId) : IDomainEvent
{
    public Guid     Id         { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record UserDeactivatedEvent(Guid UserId, Guid TenantId) : IDomainEvent
{
    public Guid     Id         { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
