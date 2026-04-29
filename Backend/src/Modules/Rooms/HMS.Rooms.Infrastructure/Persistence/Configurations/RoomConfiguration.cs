using HMS.Rooms.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Rooms.Infrastructure.Persistence.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("Rooms", schema: "rooms");

        builder.HasKey(r => r.Id);

        // ── Concurrency token — prevents double-booking race conditions ─────────
        builder.Property(r => r.RowVersion)
               .IsRowVersion()
               .IsRequired();

        builder.Property(r => r.Name)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(r => r.RoomNumber)
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(r => r.Type)
               .HasConversion<string>()
               .HasMaxLength(30);

        builder.Property(r => r.IsOccupied).IsRequired();
        builder.Property(r => r.CurrentOccupancy).IsRequired().HasDefaultValue(0);
        builder.Property(r => r.Capacity).IsRequired().HasDefaultValue(1);

        // ── Indexes ──────────────────────────────────────────────────────────────
        builder.HasIndex(r => new { r.RoomNumber, r.BranchId, r.TenantId })
               .IsUnique()
               .HasDatabaseName("IX_Rooms_RoomNumber_Branch_Tenant");

        builder.HasIndex(r => new { r.TenantId, r.BranchId, r.IsOccupied })
               .HasDatabaseName("IX_Rooms_Tenant_Branch_Occupied");

        // Soft delete + tenant filter applied by global filter
        builder.Property(r => r.IsDeleted).IsRequired().HasDefaultValue(false);
    }
}

public sealed class RoomAssignmentConfiguration : IEntityTypeConfiguration<RoomAssignment>
{
    public void Configure(EntityTypeBuilder<RoomAssignment> builder)
    {
        builder.ToTable("RoomAssignments", schema: "rooms");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.IsActive).IsRequired();
        builder.Property(a => a.AssignedAt).IsRequired();
        builder.Property(a => a.ReleasedAt);  // nullable — NEW column

        builder.HasOne(a => a.Room)
               .WithMany()
               .HasForeignKey(a => a.RoomId)
               .OnDelete(DeleteBehavior.Restrict);

        // ── Indexes ──────────────────────────────────────────────────────────────
        builder.HasIndex(a => new { a.VisitId, a.IsActive })
               .HasDatabaseName("IX_RoomAssignments_Visit_Active");

        builder.HasIndex(a => new { a.RoomId, a.IsActive })
               .HasDatabaseName("IX_RoomAssignments_Room_Active");

        builder.Property(a => a.IsDeleted).IsRequired().HasDefaultValue(false);
    }
}
