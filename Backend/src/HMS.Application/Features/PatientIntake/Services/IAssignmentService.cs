using PatientIntakeEntity = HMS.Domain.Entities.PatientIntake.PatientIntake;

namespace HMS.Application.Features.PatientIntake.Services;

public interface IAssignmentService
{
    Task<(Guid? roomId, Guid? doctorId)> AssignAsync(PatientIntakeEntity intake, Guid? requestedRoomId = null, Guid? requestedDoctorId = null, CancellationToken ct = default);
}