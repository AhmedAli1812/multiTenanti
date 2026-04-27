public class WristbandDto
{
    public string PatientName { get; set; } = default!;
    public string MedicalNumber { get; set; } = default!;
    public string? RoomNumber { get; set; }
    public byte[] QrCode { get; set; } = default!;
}