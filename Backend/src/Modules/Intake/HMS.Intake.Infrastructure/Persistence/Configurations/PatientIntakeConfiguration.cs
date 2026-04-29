using HMS.Intake.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Intake.Infrastructure.Persistence.Configurations;

public sealed class PatientIntakeConfiguration : IEntityTypeConfiguration<PatientIntake>
{
    public void Configure(EntityTypeBuilder<PatientIntake> builder)
    {
        builder.ToTable("PatientIntakes", schema: "intake");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.VisitType)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(i => i.ArrivalMethod)
               .HasConversion<string>()
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(i => i.Priority)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(i => i.PaymentType)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(i => i.ChiefComplaint)
               .HasMaxLength(1000)
               .IsRequired();

        builder.Property(i => i.Status)
               .HasConversion<string>()
               .HasMaxLength(30)
               .IsRequired();

        builder.Property(i => i.IsDeleted).IsRequired().HasDefaultValue(false);

        // ── Owned entities (Option B: normalized tables) ───────────────────────
        builder.OwnsOne(i => i.EmergencyContact, ec =>
        {
            ec.ToTable("IntakeEmergencyContacts", schema: "intake");
            ec.WithOwner().HasForeignKey("IntakeId");
            ec.HasKey("IntakeId");
            ec.Property(e => e.Name).HasMaxLength(200).IsRequired();
            ec.Property(e => e.Phone).HasMaxLength(20).IsRequired();
            ec.Property(e => e.Relationship).HasMaxLength(50);
        });

        builder.OwnsOne(i => i.Insurance, ins =>
        {
            ins.ToTable("IntakeInsuranceInfo", schema: "intake");
            ins.WithOwner().HasForeignKey("IntakeId");
            ins.HasKey("IntakeId");
            ins.Property(e => e.Provider).HasMaxLength(200).IsRequired();
            ins.Property(e => e.PolicyNumber).HasMaxLength(100).IsRequired();
            ins.Property(e => e.CoverageType).HasMaxLength(100);
        });

        builder.OwnsOne(i => i.Flags, f =>
        {
            f.ToTable("IntakeFlags", schema: "intake");
            f.WithOwner().HasForeignKey("IntakeId");
            f.HasKey("IntakeId");
            f.Property(e => e.BehaviorAlert).IsRequired();
            f.Property(e => e.FallRisk).IsRequired();
            f.Property(e => e.Dnr).IsRequired();
            f.Property(e => e.Isolation).IsRequired();
        });

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(i => new { i.TenantId, i.Status })
               .HasDatabaseName("IX_Intakes_Tenant_Status");

        builder.HasIndex(i => new { i.PatientId, i.TenantId })
               .HasDatabaseName("IX_Intakes_Patient_Tenant");
    }
}
