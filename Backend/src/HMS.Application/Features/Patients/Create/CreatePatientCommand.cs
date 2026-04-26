using HMS.Domain.Enums;
using MediatR;

public class CreatePatientCommand : IRequest<Guid>
{
    public string FullName { get; set; } = default!;
    public string MedicalNumber { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;

    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }

    public string? Email { get; set; }
    public string? NationalId { get; set; }

    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public Guid? DoctorId { get; set; }
}