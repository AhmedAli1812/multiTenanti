namespace HMS.Application.Dtos.Intake;

public class PersonalInfoDto
{
    public string FullName { get; set; } = default!;
    public string MedicalNumber { get; set; } = default!;
    public string NationalId { get; set; } = default!;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Email { get; set; }
    public string? IdCardFrontUrl { get; set; }
}