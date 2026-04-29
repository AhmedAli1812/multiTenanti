// ─────────────────────────────────────────────────────────────────────────────
// HMS.SharedKernel.Domain — BaseEntity
//
// All entities inherit from this. Domain events are collected in-memory and
// dispatched AFTER the database transaction commits (via DomainEventInterceptor).
// ─────────────────────────────────────────────────────────────────────────────
namespace HMS.SharedKernel.Primitives;

public abstract class BaseEntity : ISoftDeletable
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; protected set; } = Guid.NewGuid();

    // ── Audit ─────────────────────────────────────────────────────────────────
    public DateTime  CreatedAt  { get; set; }
    public Guid?     CreatedBy  { get; set; }
    public DateTime? UpdatedAt  { get; set; }
    public Guid?     UpdatedBy  { get; set; }

    // ── Soft delete ────────────────────────────────────────────────────────────
    public bool      IsDeleted  { get; set; } = false;
    public DateTime? DeletedAt  { get; set; }
    public Guid?     DeletedBy  { get; set; }

    // ── Domain events ──────────────────────────────────────────────────────────
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
