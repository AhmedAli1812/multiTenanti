namespace HMS.Application.Features.Patients.Common;

public class PatientDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string MedicalNumber { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
}