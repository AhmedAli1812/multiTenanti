using HMS.Application.Abstractions.Persistence;
using HMS.Application.Abstractions.Security;
using HMS.Application.Abstractions.Tenant;
using HMS.Application.Features.Users.CreateUser;
using HMS.Domain.Entities.Identity;
using HMS.Domain.Entities.Patients;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Features.Users.Create;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly ITenantProvider _tenant;

    public CreateUserHandler(
        IApplicationDbContext context,
        IPasswordHasher hasher,
        ITenantProvider tenant)
    {
        _context = context;
        _hasher = hasher;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = (request.TenantId.HasValue && _tenant.IsSuperAdmin())
            ? request.TenantId.Value
            : _tenant.GetTenantId();

        if (tenantId == null)
            throw new ArgumentException("Tenant ID is required");

        // =========================
        // 💣 VALIDATION
        // =========================
        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Full name is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Password is required");

        var email = request.Email?.Trim().ToLower();
        var username = request.Username?.Trim().ToLower();

        using var tx = await _context.BeginTransactionAsync(cancellationToken);

        // =========================
        // 🔍 EMAIL CHECK
        // =========================
        if (!string.IsNullOrEmpty(email))
        {
            var exists = await _context.Users
                .AsNoTracking()
                .AnyAsync(x => x.Email == email && x.TenantId == tenantId, cancellationToken);

            if (exists)
                throw new InvalidOperationException("Email already exists");
        }

        // =========================
        // 🔍 USERNAME CHECK
        // =========================
        if (!string.IsNullOrEmpty(username))
        {
            var exists = await _context.Users
                .AsNoTracking()
                .AnyAsync(x => x.Username == username && x.TenantId == tenantId, cancellationToken);

            if (exists)
                throw new InvalidOperationException("Username already exists");
        }

        // =========================
        // 🔥 CREATE USER
        // =========================
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Username = username,
            NationalId = request.NationalId,
            PasswordHash = _hasher.HashPassword(request.Password),
            TenantId = tenantId,
            IsActive = true,
            IsLocked = false,

            // 💣 الحل الأساسي للمشكلة
            BranchId = request.BranchId,
            DepartmentId = request.DepartmentId
        };

        await _context.Users.AddAsync(user, cancellationToken);

        // =========================
        // 🔥 GENERATE UNIQUE MRN
        // =========================
        string medicalNumber;
        int retries = 0;

        do
        {
            medicalNumber = GenerateMedicalNumber();

            var exists = await _context.Patients
                .AsNoTracking()
                .AnyAsync(x => x.MedicalNumber == medicalNumber && x.TenantId == tenantId, cancellationToken);

            if (!exists) break;

            retries++;
        }
        while (retries < 3);

        if (retries == 3)
            throw new InvalidOperationException("Failed to generate medical number");

        // =========================
        // 💣 CREATE PATIENT
        // =========================
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber ?? "",
            Email = user.Email,
            MedicalNumber = medicalNumber,
            DateOfBirth = DateTime.UtcNow,
            Gender = 0
        };

        await _context.Patients.AddAsync(patient, cancellationToken);

        // =========================
        // 🔥 VALIDATE ROLES
        // =========================
        var roleIds = request.RoleIds?.Distinct().ToList() ?? new List<Guid>();

        var roles = await _context.Roles
            .Where(r => roleIds.Contains(r.Id) && r.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        if (roles.Count != roleIds.Count)
            throw new InvalidOperationException("Invalid roles");

        // =========================
        // 🔥 ASSIGN ROLES
        // =========================
        var userRoles = roles.Select(role => new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            TenantId = tenantId,
            AssignedAt = DateTime.UtcNow
        });

        await _context.UserRoles.AddRangeAsync(userRoles, cancellationToken);

        // =========================
        // 🔥 DEFAULT PATIENT ROLE
        // =========================
        var patientRole = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.Name == "Patient" &&
                r.TenantId == tenantId,
                cancellationToken);

        if (patientRole != null && !roles.Any(r => r.Id == patientRole.Id))
        {
            await _context.UserRoles.AddAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = patientRole.Id,
                TenantId = tenantId,
                AssignedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        // =========================
        // 💾 SAVE + COMMIT
        // =========================
        await _context.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return user.Id;
    }

    private string GenerateMedicalNumber()
    {
        return $"MRN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}