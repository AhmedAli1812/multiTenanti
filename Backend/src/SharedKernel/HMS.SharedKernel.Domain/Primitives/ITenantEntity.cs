namespace HMS.SharedKernel.Primitives;

/// <summary>
/// Marks an entity as belonging to a specific tenant.
/// EF Core global query filters use this to enforce tenant isolation.
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; }
}
