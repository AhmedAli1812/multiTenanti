using HMS.Domain.Entities;
using HMS.Domain.Entities.Audit;
using HMS.Domain.Entities.Branches;
using HMS.Domain.Entities.Departments;
using HMS.Domain.Entities.Doctors;
using HMS.Domain.Entities.Identity;
using HMS.Domain.Entities.Operations;
using HMS.Domain.Entities.PatientIntake;
using HMS.Domain.Entities.Patients;
using HMS.Domain.Entities.Rooms;
using HMS.Domain.Entities.Tenancy;
using HMS.Domain.Entities.Visits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HMS.Application.Abstractions.Persistence
{
    public interface IApplicationDbContext
    {
        // =========================
        // 🔐 Identity
        // =========================
        DbSet<User> Users { get; }
        DbSet<Role> Roles { get; }
        DbSet<Permission> Permissions { get; }
        DbSet<UserRole> UserRoles { get; }
        DbSet<RolePermission> RolePermissions { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<UserSession> UserSessions { get; }

        // =========================
        // 🏥 Tenancy
        // =========================
        DbSet<HMS.Domain.Entities.Tenancy.Tenant> Tenants { get; }

        // =========================
        // 🧠 Medical
        // =========================
        DbSet<Patient> Patients { get; }
        DbSet<Visit> Visits { get; }
        DbSet<PatientIntake> Intakes { get; }
        DbSet<DoctorProfile> DoctorProfiles { get; }
        DbSet<Department> Departments { get; }

        // =========================
        // 🏢 Structure
        // =========================
        DbSet<Branch> Branches { get; }
        DbSet<Floor> Floors { get; }
        DbSet<Room> Rooms { get; }
        DbSet<RoomAssignment> RoomAssignments { get; }

        // =========================
        // 🔔 Notifications
        // =========================
        DbSet<Notification> Notifications { get; }

        // =========================
        // 📊 Audit
        // =========================
        DbSet<AuditLog> AuditLogs { get; }

        // =========================
        // 💾 Save
        // =========================
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        // =========================
        // 🔥 Transactions (IMPORTANT)
        // =========================
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}