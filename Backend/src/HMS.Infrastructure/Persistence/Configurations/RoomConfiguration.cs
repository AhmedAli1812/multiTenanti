using HMS.Domain.Entities.Rooms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        // =========================
        // 🔥 TABLE
        // =========================
        builder.ToTable("Rooms");

        // =========================
        // 🔑 KEY
        // =========================
        builder.HasKey(r => r.Id);

        // =========================
        // 📛 PROPERTIES
        // =========================
        builder.Property(r => r.RoomNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        // =========================
        // 🔗 RELATIONS
        // =========================

        builder.HasOne(r => r.Branch)
            .WithMany()
            .HasForeignKey(r => r.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Floor)
            .WithMany(f => f.Rooms)
            .HasForeignKey(r => r.FloorId)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================
        // ⚡ INDEX
        // =========================
        builder.HasIndex(r => new { r.RoomNumber, r.BranchId, r.TenantId })
            .IsUnique();
    }
}