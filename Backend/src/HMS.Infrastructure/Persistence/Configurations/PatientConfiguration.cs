using HMS.Domain.Entities.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : TenantEntityConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.MedicalNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Gender);

        // 💣 المهم
        builder.HasIndex(p => new { p.MedicalNumber, p.TenantId })
            .IsUnique();
    }
}