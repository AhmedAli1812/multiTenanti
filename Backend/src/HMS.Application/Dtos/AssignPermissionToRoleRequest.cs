public class AssignPermissionToRoleRequest
{
    public Guid RoleId { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
}