using HMS.SharedKernel.Primitives;

namespace HMS.Identity.Domain.Entities;

/// <summary>
/// User — Identity Module Aggregate Root.
///
/// DDD changes from legacy:
///   - Implements IAggregateRoot (aggregate boundary enforcement)
///   - FullName is now protected set — changed via UpdateProfile() domain method
///   - Account state changes go through domain methods: Lock(), Activate(), Deactivate()
///   - Domain events raised on state transitions
/// </summary>
public sealed class User : TenantEntity, IAggregateRoot
{
    private User() { } // EF constructor

    public static User Create(
        string fullName,
        string passwordHash,
        Guid   tenantId,
        Guid?  branchId     = null,
        Guid?  departmentId = null,
        string? email       = null,
        string? phone       = null,
        string? username    = null,
        string? nationalId  = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("User full name is required.", "USER_NAME_REQUIRED");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.", "PASSWORD_REQUIRED");

        var user = new User
        {
            Id           = Guid.NewGuid(),
            FullName     = fullName.Trim(),
            PasswordHash = passwordHash,
            TenantId     = tenantId,
            BranchId     = branchId,
            DepartmentId = departmentId,
            Email        = email?.Trim().ToLowerInvariant(),
            PhoneNumber  = phone?.Trim(),
            Username     = username?.Trim(),
            NationalId   = nationalId?.Trim(),
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow,
        };

        user.RaiseDomainEvent(new UserCreatedEvent(user.Id, tenantId));
        return user;
    }

    // ── Identity ───────────────────────────────────────────────────────────────
    public string  FullName     { get; private set; } = default!;
    public string? Email        { get; private set; }
    public string? PhoneNumber  { get; private set; }
    public string? Username     { get; private set; }
    public string? NationalId   { get; private set; }

    // ── Tenant / Organization ──────────────────────────────────────────────────
    public Guid?   BranchId     { get; private set; }
    public Guid?   DepartmentId { get; private set; }

    // ── Security ──────────────────────────────────────────────────────────────
    public string  PasswordHash                { get; private set; } = default!;
    public bool    IsActive                    { get; private set; } = true;
    public bool    IsLocked                    { get; private set; } = false;
    public DateTime? LastLoginAt               { get; private set; }
    public DateTime? LastPasswordChangedAt     { get; private set; }

    // ── Navigation ─────────────────────────────────────────────────────────────
    public ICollection<UserRole>      UserRoles     { get; private set; } = [];
    public ICollection<RefreshToken>  RefreshTokens { get; private set; } = [];

    // ── Domain methods ─────────────────────────────────────────────────────────
    public void UpdateProfile(string fullName, string? email, string? phone)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name cannot be empty.", "USER_NAME_REQUIRED");

        FullName    = fullName.Trim();
        Email       = email?.Trim().ToLowerInvariant();
        PhoneNumber = phone?.Trim();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash cannot be empty.");

        PasswordHash            = newPasswordHash;
        LastPasswordChangedAt   = DateTime.UtcNow;
    }

    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;

    public void Lock()
    {
        if (IsLocked) return;
        IsLocked = true;
        RaiseDomainEvent(new UserLockedEvent(Id, TenantId));
    }

    public void Unlock()    => IsLocked = false;
    public void Activate()  => IsActive = true;
    public void Deactivate()
    {
        IsActive = false;
        RaiseDomainEvent(new UserDeactivatedEvent(Id, TenantId));
    }

    public void AssignToBranch(Guid branchId)     => BranchId     = branchId;
    public void AssignToDepartment(Guid deptId)   => DepartmentId = deptId;
}
