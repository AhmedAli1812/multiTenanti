using HMS.Domain.Entities.Base;

namespace HMS.Domain.Entities.Identity;

public class UserRole: TenantEntity, ITenantEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;

    public Guid? TenantId { get; set; }

    public DateTime AssignedAt { get; set; }
    public Guid AssignedBy { get; set; }
}