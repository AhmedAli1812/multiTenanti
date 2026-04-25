public class PatientSearchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? MedicalNumber { get; set; }
}