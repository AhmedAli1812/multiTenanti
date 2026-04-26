using HMS.Domain.Entities.Base;
using HMS.Domain.Entities.Identity;
using HMS.Domain.Entities.Visits;
using HMS.Domain.Enums;

namespace HMS.Domain.Entities.Patients;
public class Patient : TenantEntity, ITenantEntity
{
    public string FullName { get; set; } = default!;
    public string MedicalNumber { get; set; } = default!;

    public string PhoneNumber { get; set; } = default!;
    public string? Email { get; set; }

    public Gender Gender { get; set; } // 🔥 بدل string
    public DateTime DateOfBirth { get; set; }

    public string? NationalId { get; set; }

    public string? Address { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // 📁 Files
    public string? IdCardFrontUrl { get; set; }
    public string? IdCardBackUrl { get; set; }


    // 🔗 Navigation
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}