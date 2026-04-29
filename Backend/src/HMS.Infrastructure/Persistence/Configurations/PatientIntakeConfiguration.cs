using HMS.Domain.Entities.PatientIntake;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Infrastructure.Persistence.Configurations;

public class PatientIntakeConfiguration : IEntityTypeConfiguration<PatientIntake>
{
    public void Configure(EntityTypeBuilder<PatientIntake> builder)
    {
        builder.ToTable("Intakes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChiefComplaint)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.Status);

        // Composite index: most queries filter by TenantId + Status
        builder.HasIndex(x => new { x.TenantId, x.Status });
    }
}