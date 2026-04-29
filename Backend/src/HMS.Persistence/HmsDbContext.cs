// ─────────────────────────────────────────────────────────────────────────────
// HmsDbContext — Composition root for ALL module entities.
//
// Dependency chain (no circular deps):
//   SharedKernel.Domain ← SharedKernel.Application ← SharedKernel.Infrastructure
//   Module.Domain ← Module.Application ← (Module.Infrastructure — NOT referenced here)
//   HMS.Persistence → SharedKernel.Infrastructure + Module.Application layers only
// ─────────────────────────────────────────────────────────────────────────────
using HMS.Identity.Application.Abstractions;
using HMS.Identity.Domain.Entities;
using HMS.Intake.Application.Abstractions;
using HMS.Intake.Domain.Entities;
using HMS.Patients.Domain.Entities;
using HMS.Rooms.Application.Abstractions;
using HMS.Rooms.Domain.Entities;
using HMS.SharedKernel.Infrastructure.Persistence;
using HMS.SharedKernel.Infrastructure.Tenancy;
using HMS.Visits.Application.Abstractions;
using HMS.Visits.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HMS.Persistence;

public sealed class HmsDbContext :
    ApplicationDbContextBase,
    IIdentityDbContext,
    IVisitsDbContext,
    IRoomsDbContext,
    IIntakeDbContext
{
    public HmsDbContext(
        DbContextOptions<HmsDbContext> options,
        ITenantProvider tenantProvider)
        : base(options, tenantProvider) { }

    // ── Identity ───────────────────────────────────────────────────────────────
    public DbSet<User>           Users           => Set<User>();
    public DbSet<Role>           Roles           => Set<Role>();
    public DbSet<Permission>     Permissions     => Set<Permission>();
    public DbSet<UserRole>       UserRoles       => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken>   RefreshTokens   => Set<RefreshToken>();
    public DbSet<UserSession>    UserSessions    => Set<UserSession>();

    // ── Patients ───────────────────────────────────────────────────────────────
    public DbSet<Patient>        Patients        => Set<Patient>();

    // ── Rooms ──────────────────────────────────────────────────────────────────
    public DbSet<Room>           Rooms           => Set<Room>();
    public DbSet<RoomAssignment> RoomAssignments => Set<RoomAssignment>();

    // ── Intake ─────────────────────────────────────────────────────────────────
    public DbSet<PatientIntake>  Intakes         => Set<PatientIntake>();

    // ── Visits ─────────────────────────────────────────────────────────────────
    public DbSet<Visit>          Visits          => Set<Visit>();

    // ── Transaction support ────────────────────────────────────────────────────
    public new async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => await Database.BeginTransactionAsync(ct);

    // ── Model configuration ────────────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // All EF configurations live in HMS.Persistence/Configurations/
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HmsDbContext).Assembly);

        base.OnModelCreating(modelBuilder); // applies global tenant + soft-delete filters
    }
}
