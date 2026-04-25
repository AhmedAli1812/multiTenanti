using HMS.Application.Dtos.Intake;
using MediatR;

namespace HMS.Application.Features.PatientIntake.Commands.SubmitIntake;

public class SubmitIntakeCommand : IRequest<WristbandDto>
{
    public Guid IntakeId { get; set; }      // 🔥 مهم
    public Guid TenantId { get; set; }      // 🔥 مهم (عشان TenantEntity)
    public Guid? DoctorId { get; set; }
    public Guid? RoomId { get; set; }
    public PersonalInfoDto PersonalInfo { get; set; } = default!;
    public EmergencyContactDto EmergencyContact { get; set; } = default!;
    public ContactPreferencesDto ContactPreferences { get; set; } = default!;
    public VisitInfoDto VisitInfo { get; set; } = default!;
    public PaymentDto Payment { get; set; } = default!;
    public ConsentDto Consent { get; set; } = default!;
    public FlagsDto Flags { get; set; } = default!;
}