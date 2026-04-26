using HMS.Domain.Entities.Base;

namespace HMS.Domain.Entities.Identity;

public class Role : TenantEntity, ITenantEntity
{
    public string Name { get; set; } = default!;

    public bool IsSystem { get; set; } = false;

    // 🔗 Relations
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}