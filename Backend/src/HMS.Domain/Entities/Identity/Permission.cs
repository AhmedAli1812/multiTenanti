using HMS.Domain.Entities.Base;

namespace HMS.Domain.Entities.Identity
{
        public class Permission
    {
        public Guid Id { get; set; }

        public string Code { get; set; } = default!;
        // مثال: users.create

        public string Module { get; set; } = default!;
        // Users / Visits / Reports

        public string Action { get; set; } = default!;
        // view / create / edit / delete

        public string Description { get; set; } = default!;

        // 🔗 Relations
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}