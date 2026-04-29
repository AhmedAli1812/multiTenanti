using HMS.Intake.Domain.Entities;
using HMS.Patients.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HMS.Intake.Application.Abstractions;

/// <summary>Intake module slice of HmsDbContext.</summary>
public interface IIntakeDbContext
{
    DbSet<PatientIntake> Intakes  { get; }
    DbSet<Patient>       Patients { get; }   // read-only cross-module access
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
