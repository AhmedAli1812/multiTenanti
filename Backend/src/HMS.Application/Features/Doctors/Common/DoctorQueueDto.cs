namespace HMS.Application.Features.Doctors.Common;

public class DoctorQueueDto
{
    public Guid VisitId { get; set; }
    public string PatientName { get; set; } = default!;
    public int QueueNumber { get; set; }
    public string Status { get; set; } = default!;
}