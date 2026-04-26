using HMS.Domain.Entities.Base;

namespace HMS.Domain.Entities.Identity;

public class RolePermission 
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = default!;
    public Guid? TenantId { get; set; }
}