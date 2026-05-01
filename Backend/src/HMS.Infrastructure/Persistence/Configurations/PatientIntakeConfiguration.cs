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

        builder.Property(x => x.Notes)
            .HasMaxLength(4000);

        builder.Property(x => x.Status);
        builder.Property(x => x.VisitType);
        builder.Property(x => x.Priority);
        builder.Property(x => x.ArrivalMethod);
        builder.Property(x => x.PaymentType);

        builder.HasIndex(x => x.TenantId);
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.Status);

        // Composite index: most queries filter by TenantId + Status
        builder.HasIndex(x => new { x.TenantId, x.Status });
    }
}