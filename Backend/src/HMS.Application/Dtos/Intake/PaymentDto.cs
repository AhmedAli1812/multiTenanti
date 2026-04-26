namespace HMS.Application.Dtos.Intake;

public class PaymentDto
{
    public string PaymentType { get; set; } = default!;
    public string? Company { get; set; }
    public string? PolicyNumber { get; set; }
    public string? Class { get; set; }
}