using HMS.Application.Dtos;
using HMS.Application.Dtos.Intake;
using MediatR;

public class SubmitIntakeCommand : IRequest<WristbandDto> // 🔥 FIX مهم
{
    public Guid IntakeId { get; set; }
    public Guid TenantId { get; set; }

    public PersonalInfoDto PersonalInfo { get; set; }
    public EmergencyContactDto EmergencyContact { get; set; }
    public ContactPreferencesDto ContactPreferences { get; set; }
    public VisitInfoDto VisitInfo { get; set; }
    public PaymentDto Payment { get; set; }
    public ConsentDto Consent { get; set; }
    public FlagsDto Flags { get; set; }
}