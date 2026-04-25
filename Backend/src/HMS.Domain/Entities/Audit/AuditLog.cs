using HMS.Domain.Entities.Base;

namespace HMS.Domain.Entities.Audit;

public class AuditLog : TenantEntity, ITenantEntity
{

    public Guid Id { get; set; }

    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }

    public string Action { get; set; } = default!; // CREATE / UPDATE / DELETE

    public string EntityName { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public string? UserName { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }

    public string? IPAddress { get; set; }

    public DateTime CreatedAt { get; set; }
}