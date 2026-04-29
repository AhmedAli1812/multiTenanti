using HMS.Visits.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Visits.Infrastructure.Persistence.Configurations;

public sealed class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("Visits", schema: "visits");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.VisitType)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(v => v.PayerType)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(v => v.Status)
               .HasConversion<string>()
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(v => v.QueueNumber).IsRequired();
        builder.Property(v => v.VisitDate).IsRequired();
        builder.Property(v => v.IsDeleted).IsRequired().HasDefaultValue(false);

        // ── Indexes ────────────────────────────────────────────────────────────
        builder.HasIndex(v => new { v.TenantId, v.Status })
               .HasDatabaseName("IX_Visits_Tenant_Status");

        builder.HasIndex(v => new { v.PatientId, v.TenantId })
               .HasDatabaseName("IX_Visits_Patient_Tenant");

        builder.HasIndex(v => new { v.BranchId, v.TenantId, v.VisitDate })
               .HasDatabaseName("IX_Visits_Branch_Tenant_Date");

        builder.HasIndex(v => new { v.DoctorId, v.TenantId, v.Status })
               .HasDatabaseName("IX_Visits_Doctor_Tenant_Status");
    }
}
