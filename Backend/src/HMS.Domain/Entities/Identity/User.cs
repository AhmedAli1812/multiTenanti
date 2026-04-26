using HMS.Domain.Entities.Base;
using HMS.Domain.Entities.Branches;
using HMS.Domain.Entities.Departments;
using HMS.Domain.Entities.Patients;

namespace HMS.Domain.Entities.Identity
{
    public class User : TenantEntity, ITenantEntity
    {
        // 🔹 Basic Info
        public string FullName { get; set; } = default!;

        // 🔹 Login Identifiers (كلهم optional بس لازم واحد منهم)
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Username { get; set; }
        public string? NationalId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public Guid? BranchId { get; set; }
        public Branch? Branch { get; set; }
        // 🔐 Security
        public string PasswordHash { get; set; } = default!;

        // 🔹 Account Status
        public bool IsActive { get; set; } = true;
        public bool IsLocked { get; set; } = false;

        // 🗑️ Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // 🕓 Tracking
        public DateTime? LastLoginAt { get; set; }
        public DateTime? LastPasswordChangedAt { get; set; }

        // 🔗 Relations

        // UserRoles (Many-to-Many)
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        // Refresh Tokens
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}