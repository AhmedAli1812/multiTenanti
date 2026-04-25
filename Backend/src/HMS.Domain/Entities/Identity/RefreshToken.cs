using HMS.Domain.Entities.Base;

namespace HMS.Domain.Entities.Identity
{
    public class RefreshToken : TenantEntity, ITenantEntity
    {
        public Guid UserId { get; set; }

        // 🔐 هنخزن hashed token مش raw
        public string Token { get; set; } = default!;

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; } = false;

        public string? DeviceInfo { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ReplacedByToken { get; set; }
        // 🔗 Navigation
        public User User { get; set; } = default!;
        public string? ReplacedByTokenHash { get; set; }
    }
}