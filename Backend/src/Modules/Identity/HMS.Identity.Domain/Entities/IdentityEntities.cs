using HMS.SharedKernel.Primitives;

namespace HMS.Identity.Domain.Entities;

public sealed class Role : TenantEntity
{
    public string  Name     { get; set; } = default!;
    public bool    IsSystem { get; set; } = false;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    public ICollection<UserRole>       UserRoles        { get; set; } = [];
}

public sealed class Permission : BaseEntity
{
    public string Name        { get; set; } = default!;
    public string? GroupName  { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}

public sealed class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public User User   { get; set; } = default!;
    public Guid RoleId { get; set; }
    public Role Role   { get; set; } = default!;
}

public sealed class RolePermission : BaseEntity
{
    public Guid       RoleId       { get; set; }
    public Role       Role         { get; set; } = default!;
    public Guid       PermissionId { get; set; }
    public Permission Permission   { get; set; } = default!;
}

public sealed class RefreshToken : BaseEntity
{
    public string  Token        { get; set; } = default!;
    public string? DeviceId     { get; set; }
    public DateTime ExpiresAt   { get; set; }
    public bool     IsRevoked   { get; set; } = false;
    public Guid     UserId      { get; set; }
    public User     User        { get; set; } = default!;

    public bool IsExpired  => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive   => !IsRevoked && !IsExpired;

    public void Revoke() => IsRevoked = true;
}

public sealed class UserSession : BaseEntity
{
    public Guid     UserId      { get; set; }
    public string   DeviceId    { get; set; } = default!;
    public DateTime LastSeen    { get; set; } = DateTime.UtcNow;
}
