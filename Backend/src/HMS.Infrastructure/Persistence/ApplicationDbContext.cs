using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Tenant;
using HMS.Application.Abstractions.CurrentUser;
using HMS.Domain.Entities;
using HMS.Domain.Entities.Audit;
using HMS.Domain.Entities.Base;
using HMS.Domain.Entities.Branches;
using HMS.Domain.Entities.Doctors;
using HMS.Domain.Entities.Identity;
using HMS.Domain.Entities.Operations;
using HMS.Domain.Entities.PatientIntake;
using HMS.Domain.Entities.Patients;
using HMS.Domain.Entities.Tenancy;
using HMS.Domain.Entities.Visits;
using HMS.Domain.Entities.Rooms;
using HMS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;

namespace HMS.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUser _currentUser;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantProvider tenantProvider,
        ICurrentUser currentUser) : base(options)
    {
        _tenantProvider = tenantProvider;
        _currentUser = currentUser;
    }

    // =========================
    // DB SETS
    // =========================
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<PatientIntake> Intakes => Set<PatientIntake>();
    public DbSet<RoomAssignment> RoomAssignments => Set<RoomAssignment>();

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Patient> Patients => Set<Patient>();

    // =========================
    // 🔥 TRANSACTION SUPPORT
    // =========================
    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct)
    {
        return await Database.BeginTransactionAsync(ct);
    }

    // =========================
    // MODEL CONFIG
    // =========================
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        modelBuilder.Entity<Branch>().ToTable("Branch");

        // =========================
        // RELATIONS
        // =========================
        modelBuilder.Entity<Room>()
            .HasOne(r => r.Branch)
            .WithMany()
            .HasForeignKey(r => r.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Visit>()
            .HasOne(v => v.Branch)
            .WithMany()
            .HasForeignKey(v => v.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RoomAssignment>()
            .HasOne(a => a.Room)
            .WithMany()
            .HasForeignKey(a => a.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RoomAssignment>()
            .HasOne(a => a.Visit)
            .WithMany()
            .HasForeignKey(a => a.VisitId)
            .OnDelete(DeleteBehavior.Restrict);

        ApplyIndexes(modelBuilder);
        ApplyGlobalFilters(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    // =========================
    // 🔥 GLOBAL FILTER (FIXED 💣)
    // =========================
    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
            {
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(SetGlobalFilter), BindingFlags.NonPublic | BindingFlags.Instance)!
                    .MakeGenericMethod(clrType);

                method.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    private void SetGlobalFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class
    {
        if (typeof(ISoftDeletable).IsAssignableFrom(typeof(TEntity)))
        {
            modelBuilder.Entity<TEntity>()
                .HasQueryFilter(e =>
                    (
                        _tenantProvider.IsSuperAdmin()
                        ||
                        EF.Property<Guid?>(e, "TenantId") == _tenantProvider.TryGetTenantId()
                    )
                    &&
                    EF.Property<bool>(e, "IsDeleted") == false
                );
        }
        else
        {
            modelBuilder.Entity<TEntity>()
                .HasQueryFilter(e =>
                    _tenantProvider.IsSuperAdmin()
                    ||
                    EF.Property<Guid?>(e, "TenantId") == _tenantProvider.TryGetTenantId()
                );
        }
    }

    // =========================
    // 🔥 INDEXES
    // =========================
    private void ApplyIndexes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>()
            .HasIndex(x => new { x.MedicalNumber, x.TenantId })
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(x => new { x.Email, x.TenantId }).IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(x => new { x.Username, x.TenantId }).IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(x => new { x.PhoneNumber, x.TenantId }).IsUnique();

        modelBuilder.Entity<Room>()
            .HasIndex(x => new { x.RoomNumber, x.BranchId, x.TenantId })
            .IsUnique();
    }

    // =========================
    // 🔥 AUDIT SYSTEM
    // =========================
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;

        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = userId;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = userId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}