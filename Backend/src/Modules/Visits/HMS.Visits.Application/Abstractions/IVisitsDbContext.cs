using HMS.Intake.Domain.Entities;
using HMS.Identity.Domain.Entities;
using HMS.Rooms.Domain.Entities;
using HMS.Visits.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HMS.Visits.Application.Abstractions;

/// <summary>
/// Visits module slice of the shared DbContext.
///
/// Includes cross-module read access to Rooms, Users, and Intakes
/// because the IntakeSubmittedEventHandler needs them for room assignment
/// and visit creation. This is intentional — in a Modular Monolith, read
/// access across module boundaries is acceptable; only write access to
/// another module's aggregate root is prohibited.
/// </summary>
public interface IVisitsDbContext
{
    DbSet<Visit>          Visits          { get; }
    DbSet<RoomAssignment> RoomAssignments { get; }

    // Read-only cross-module access (intentional — see ADR-2)
    DbSet<Room>           Rooms           { get; }
    DbSet<User>           Users           { get; }
    DbSet<PatientIntake>  Intakes         { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}

/// <summary>
/// Minimal current-user context for visit handlers.
/// Avoids HttpContext dependency inside Application layer.
/// </summary>
public interface ICurrentUserContext
{
    Guid? UserId   { get; }
    Guid? TenantId { get; }
    bool  IsGlobal { get; }
}
