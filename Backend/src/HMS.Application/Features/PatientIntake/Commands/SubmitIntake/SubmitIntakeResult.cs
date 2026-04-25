namespace HMS.Application.Features.PatientIntake.Commands.SubmitIntake;

public class SubmitIntakeResult
{
    public Guid PatientId { get; set; }
    public string MedicalNumber { get; set; } = default!;
    public string QrCode { get; set; } = default!;
}