public class VisitListDto
{
    public Guid Id { get; set; }

    public string PatientName { get; set; } = default!;
    public string MedicalNumber { get; set; } = default!;

    public string DoctorName { get; set; } = default!;
    public string Room { get; set; } = default!;
    public string BranchName { get; set; } = default!;

    public string Status { get; set; } = default!;
    public DateTime StartedAt { get; set; }
}