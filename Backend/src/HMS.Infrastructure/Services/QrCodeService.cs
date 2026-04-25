using QRCoder;

public class QrCodeService : IQrCodeService
{
    public byte[] Generate(string content)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);

        return qrCode.GetGraphic(20);
    }
}