using HMS.Domain.Entities.PatientIntake;

public interface IAssignmentService
{
    Task<(Guid roomId, Guid doctorId)> AssignAsync(PatientIntake intake, CancellationToken ct);
}