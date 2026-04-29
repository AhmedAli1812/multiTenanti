using HMS.SharedKernel.Primitives;
using HMS.Visits.Domain.Entities;

namespace HMS.Visits.Domain.Events;


public sealed record VisitCreatedEvent(
    Guid VisitId,
    Guid PatientId,
    Guid TenantId,
    Guid BranchId) : IDomainEvent
{
    public Guid     Id         { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record VisitStatusChangedEvent(
    Guid        VisitId,
    Guid        TenantId,
    VisitStatus OldStatus,
    VisitStatus NewStatus) : IDomainEvent
{
    public Guid     Id         { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record VisitCompletedEvent(
    Guid VisitId,
    Guid PatientId,
    Guid TenantId) : IDomainEvent
{
    public Guid     Id         { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
