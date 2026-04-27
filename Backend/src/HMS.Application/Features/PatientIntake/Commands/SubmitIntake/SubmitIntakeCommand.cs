using HMS.Application.Dtos;
using HMS.Application.Dtos.Intake;
using MediatR;

namespace HMS.Application.Features.Reception.Intake.Commands;

public class SubmitIntakeCommand : IRequest<WristbandDto>
{
    public Guid IntakeId { get; set; }
    public Guid TenantId { get; set; }

    // =========================
    // 🧾 DTOs (Not Null Safe)
    // =========================
    public PersonalInfoDto PersonalInfo { get; set; } = new();
    public EmergencyContactDto EmergencyContact { get; set; } = new();
    public ContactPreferencesDto ContactPreferences { get; set; } = new();
    public VisitInfoDto VisitInfo { get; set; } = new();
    public PaymentDto Payment { get; set; } = new();
    public ConsentDto Consent { get; set; } = new();
    public FlagsDto Flags { get; set; } = new();

    // =========================
    // 🛡️ Validation Helper
    // =========================
    public bool IsValid()
    {
        if (TenantId == Guid.Empty)
            return false;

        if (PersonalInfo == null || string.IsNullOrWhiteSpace(PersonalInfo.FullName))
            return false;

        if (VisitInfo == null || VisitInfo.BranchId == Guid.Empty)
            return false;

        return true;
    }
}