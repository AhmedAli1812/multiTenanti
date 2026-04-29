using HMS.Identity.Domain.Entities;
using HMS.Intake.Domain.Entities;
using HMS.Patients.Domain.Entities;
using HMS.Rooms.Domain.Entities;
using HMS.Visits.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Persistence.Configurations;

// ── IDENTITY ──────────────────────────────────────────────────────────────────

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users", "identity");
        b.HasKey(u => u.Id);
        b.Property(u => u.Email).HasMaxLength(256);
        b.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        b.Property(u => u.Username).HasMaxLength(100);
        b.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
        b.Property(u => u.PhoneNumber).HasMaxLength(20);
        b.HasIndex(u => new { u.TenantId, u.Email });
        b.HasIndex(u => new { u.TenantId, u.Username });
    }
}

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("Roles", "identity");
        b.HasKey(r => r.Id);
        b.Property(r => r.Name).HasMaxLength(100).IsRequired();
        b.HasIndex(r => new { r.TenantId, r.Name }).IsUnique();
    }
}

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.ToTable("Permissions", "identity");
        b.HasKey(p => p.Id);
        b.Property(p => p.Name).HasMaxLength(100).IsRequired();
        b.HasIndex(p => p.Name).IsUnique();
    }
}

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.ToTable("UserRoles", "identity");
        b.HasKey(ur => new { ur.UserId, ur.RoleId });
    }
}

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> b)
    {
        b.ToTable("RolePermissions", "identity");
        b.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // Fix: Role has a global query filter; make FK optional so EF
        // doesn't warn about mismatched filters on the required end.
        b.HasOne(rp => rp.Role)
         .WithMany(r => r.RolePermissions)
         .HasForeignKey(rp => rp.RoleId)
         .IsRequired(false)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("RefreshTokens", "identity");
        b.HasKey(rt => rt.Id);
        b.Property(rt => rt.Token).HasMaxLength(512).IsRequired();
        b.Property(rt => rt.DeviceId).HasMaxLength(256);
        b.HasIndex(rt => rt.Token).IsUnique();
    }
}

internal sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> b)
    {
        b.ToTable("UserSessions", "identity");
        b.HasKey(s => s.Id);
        b.Property(s => s.DeviceId).HasMaxLength(256);

        // Fix: User has a global query filter; make the FK optional so EF
        // doesn't warn about mismatched filters on the required end.
        // UserSession lives in the Identity module entity — no cross-module nav prop.
        // Use the generic HasOne<User>() overload to configure the FK without requiring
        // a User navigation property on UserSession.
        b.HasOne<User>()
         .WithMany()
         .HasForeignKey(s => s.UserId)
         .IsRequired(false)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

// ── PATIENTS ──────────────────────────────────────────────────────────────────

internal sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> b)
    {
        b.ToTable("Patients", "patients");
        b.HasKey(p => p.Id);
        b.Property(p => p.FullName).HasMaxLength(200).IsRequired();
        b.Property(p => p.MedicalNumber).HasMaxLength(50).IsRequired();
        b.Property(p => p.PhoneNumber).HasMaxLength(20);
        b.Property(p => p.Email).HasMaxLength(256);
        b.Property(p => p.NationalId).HasMaxLength(50);
        b.HasIndex(p => new { p.TenantId, p.MedicalNumber }).IsUnique();
    }
}

// ── ROOMS ─────────────────────────────────────────────────────────────────────

internal sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> b)
    {
        b.ToTable("Rooms", "rooms");
        b.HasKey(r => r.Id);
        b.Property(r => r.Name).HasMaxLength(100).IsRequired();
        b.Property(r => r.RoomNumber).HasMaxLength(20).IsRequired();
        b.Property(r => r.Type).HasConversion<string>().HasMaxLength(50);
        b.HasIndex(r => new { r.TenantId, r.BranchId, r.RoomNumber }).IsUnique();
        // Optimistic concurrency — prevents double-booking
        b.Property(r => r.RowVersion).IsRowVersion();
    }
}

internal sealed class RoomAssignmentConfiguration : IEntityTypeConfiguration<RoomAssignment>
{
    public void Configure(EntityTypeBuilder<RoomAssignment> b)
    {
        b.ToTable("RoomAssignments", "rooms");
        b.HasKey(ra => ra.Id);
        b.HasIndex(ra => new { ra.VisitId, ra.RoomId });
    }
}

// ── INTAKE ────────────────────────────────────────────────────────────────────

internal sealed class PatientIntakeConfiguration : IEntityTypeConfiguration<PatientIntake>
{
    public void Configure(EntityTypeBuilder<PatientIntake> b)
    {
        b.ToTable("Intakes", "intake");
        b.HasKey(i => i.Id);
        b.Property(i => i.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(i => i.VisitType).HasConversion<string>().HasMaxLength(50);
        b.Property(i => i.Priority).HasConversion<string>().HasMaxLength(50);
        b.Property(i => i.ArrivalMethod).HasConversion<string>().HasMaxLength(50);
        b.Property(i => i.ChiefComplaint).HasMaxLength(1000);

        // Owned entities (Option B — normalized tables)
        b.OwnsOne(i => i.EmergencyContact, ec =>
        {
            ec.ToTable("IntakeEmergencyContacts", "intake");
            ec.Property(e => e.Name).HasMaxLength(200);
            ec.Property(e => e.Phone).HasMaxLength(20);
            ec.Property(e => e.Relationship).HasMaxLength(100);
        });

        b.OwnsOne(i => i.Insurance, ins =>
        {
            ins.ToTable("IntakeInsurance", "intake");
            ins.Property(e => e.Provider).HasMaxLength(200);
            ins.Property(e => e.PolicyNumber).HasMaxLength(100);
            ins.Property(e => e.CoverageType).HasMaxLength(100);
        });

        b.OwnsOne(i => i.Flags, f =>
        {
            f.ToTable("IntakeFlags", "intake");
        });

        b.HasIndex(i => new { i.TenantId, i.PatientId });
        b.HasIndex(i => new { i.TenantId, i.Status });
    }
}

// ── VISITS ────────────────────────────────────────────────────────────────────

internal sealed class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> b)
    {
        b.ToTable("Visits", "visits");
        b.HasKey(v => v.Id);
        b.Property(v => v.Status).HasConversion<string>().HasMaxLength(50);
        b.Property(v => v.VisitType).HasConversion<string>().HasMaxLength(50);
        b.HasIndex(v => new { v.TenantId, v.BranchId, v.VisitDate });
        b.HasIndex(v => new { v.TenantId, v.PatientId });
        b.HasIndex(v => new { v.TenantId, v.Status });
        b.HasIndex(v => new { v.TenantId, v.BranchId, v.QueueNumber });

        // Visit lives in the Visits module — no cross-module Patient nav prop (boundary rule).
        // Use the generic HasOne<Patient>() overload to configure the FK without requiring
        // a Patient navigation property on Visit. IsRequired(false) fixes the global
        // query filter mismatch (Patient has a filter; Visit is the dependent end).
        b.HasOne<Patient>()
         .WithMany()
         .HasForeignKey(v => v.PatientId)
         .IsRequired(false)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
