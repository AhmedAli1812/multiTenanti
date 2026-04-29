using HMS.SharedKernel.Primitives;

namespace HMS.Patients.Domain.Entities;

public sealed record PatientRegisteredEvent(
    Guid   PatientId,
    Guid   TenantId,
    string MedicalNumber) : IDomainEvent
{
    public Guid     Id         { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record PatientUpdatedEvent(Guid PatientId, Guid TenantId) : IDomainEvent
{
    public Guid     Id         { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
