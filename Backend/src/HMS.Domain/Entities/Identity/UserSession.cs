using HMS.Domain.Entities.Base;

namespace HMS.Domain.Entities.Identity;

public class UserSession : TenantEntity
{
    public Guid UserId { get; set; }

    public string DeviceId { get; set; } = default!;

    public string? DeviceName { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public bool IsRevoked { get; set; } = false;

    public DateTime LastActivityAt { get; set; }

    // 🔗 Navigation
    public User? User { get; set; }
}