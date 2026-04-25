using HMS.Domain.Entities.Base;

namespace HMS.Domain.Entities.Doctors;

public class DoctorProfile : TenantEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; } // 🔗 مع User

    public string Specialty { get; set; } = default!;
    public int YearsOfExperience { get; set; }

    // Navigation (اختياري)
    // public User User { get; set; }
}