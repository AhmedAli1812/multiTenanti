using HMS.Domain.Entities.Visits;
using HMS.Domain.Entities.Identity;
using HMS.Domain.Entities.Patients;
using HMS.Domain.Entities.Branches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HMS.Infrastructure.Persistence.Configurations;

public class VisitConfiguration : IEntityTypeConfiguration<Visit>
{
    public void Configure(EntityTypeBuilder<Visit> builder)
    {
        builder.ToTable("Visits");

        // 🔑 PK
        builder.HasKey(v => v.Id);

        // =========================
        // 🔗 Relationships
        // =========================

        // Patient (Required)
        builder.HasOne(v => v.Patient)
            .WithMany()
            .HasForeignKey(v => v.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Doctor (Optional)
        builder.HasOne(v => v.Doctor)
            .WithMany()
            .HasForeignKey(v => v.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Branch (Required)
        builder.HasOne(v => v.Branch)
            .WithMany()
            .HasForeignKey(v => v.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // =========================
        // 📊 Properties
        // =========================

        builder.Property(v => v.VisitType).IsRequired();
        builder.Property(v => v.PayerType).IsRequired();
        builder.Property(v => v.Status).IsRequired();
        builder.Property(v => v.QueueNumber).IsRequired();
        builder.Property(v => v.VisitDate).IsRequired();

        builder.Property(v => v.StartedAt).IsRequired(false);
        builder.Property(v => v.CompletedAt).IsRequired(false);

        // =========================
        // 🔍 Indexes
        // =========================

        builder.HasIndex(v => v.PatientId);
        builder.HasIndex(v => v.DoctorId);
        builder.HasIndex(v => v.BranchId);
        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => v.VisitDate);
        builder.HasIndex(v => v.TenantId);
    }
}