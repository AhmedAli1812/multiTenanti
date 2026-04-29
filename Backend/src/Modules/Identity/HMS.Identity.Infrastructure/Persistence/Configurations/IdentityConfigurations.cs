using HMS.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", schema: "identity");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(200);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.Username).HasMaxLength(100);
        builder.Property(u => u.NationalId).HasMaxLength(50);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(u => u.IsLocked).IsRequired().HasDefaultValue(false);
        builder.Property(u => u.IsDeleted).IsRequired().HasDefaultValue(false);

        // ── Unique indexes ─────────────────────────────────────────────────────
        builder.HasIndex(u => new { u.Email,       u.TenantId }).IsUnique().HasDatabaseName("IX_Users_Email_Tenant");
        builder.HasIndex(u => new { u.Username,    u.TenantId }).IsUnique().HasDatabaseName("IX_Users_Username_Tenant");
        builder.HasIndex(u => new { u.PhoneNumber, u.TenantId }).IsUnique().HasDatabaseName("IX_Users_Phone_Tenant");

        // ── Relations ──────────────────────────────────────────────────────────
        builder.HasMany(u => u.UserRoles)
               .WithOne(ur => ur.User)
               .HasForeignKey(ur => ur.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RefreshTokens)
               .WithOne(rt => rt.User)
               .HasForeignKey(rt => rt.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles", schema: "identity");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
        builder.Property(r => r.IsSystem).IsRequired().HasDefaultValue(false);
        builder.Property(r => r.IsDeleted).IsRequired().HasDefaultValue(false);
    }
}

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions", schema: "identity");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.GroupName).HasMaxLength(100);
        builder.HasIndex(p => p.Name).IsUnique().HasDatabaseName("IX_Permissions_Name");
    }
}

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles", schema: "identity");
        builder.HasKey(ur => ur.Id);
        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
               .IsUnique()
               .HasDatabaseName("IX_UserRoles_User_Role");
    }
}

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions", schema: "identity");
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", schema: "identity");
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Token).HasMaxLength(500).IsRequired();
        builder.Property(rt => rt.DeviceId).HasMaxLength(200);
        builder.HasIndex(rt => new { rt.Token, rt.UserId })
               .HasDatabaseName("IX_RefreshTokens_Token_User");
    }
}
