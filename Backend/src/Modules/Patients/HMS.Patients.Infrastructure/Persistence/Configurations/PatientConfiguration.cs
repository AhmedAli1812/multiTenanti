using HMS.Patients.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Patients.Infrastructure.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients", schema: "patients");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.FullName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.MedicalNumber).HasMaxLength(50).IsRequired();
        builder.Property(p => p.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.Property(p => p.Gender).HasMaxLength(10).IsRequired();
        builder.Property(p => p.DateOfBirth).IsRequired();
        builder.Property(p => p.NationalId).HasMaxLength(50);
        builder.Property(p => p.Address).HasMaxLength(500);
        builder.Property(p => p.EmergencyContactName).HasMaxLength(200);
        builder.Property(p => p.EmergencyContactPhone).HasMaxLength(20);
        builder.Property(p => p.IdCardFrontUrl).HasMaxLength(500);
        builder.Property(p => p.IdCardBackUrl).HasMaxLength(500);
        builder.Property(p => p.IsDeleted).IsRequired().HasDefaultValue(false);

        // ── Unique indexes ─────────────────────────────────────────────────────
        builder.HasIndex(p => new { p.MedicalNumber, p.TenantId })
               .IsUnique()
               .HasDatabaseName("IX_Patients_MedicalNumber_Tenant");

        builder.HasIndex(p => new { p.TenantId, p.IsDeleted })
               .HasDatabaseName("IX_Patients_Tenant_Deleted");
    }
}
