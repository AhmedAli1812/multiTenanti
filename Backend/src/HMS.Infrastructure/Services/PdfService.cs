using HMS.Application.Abstractions.Services;
using HMS.Application.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HMS.Infrastructure.Services;

public class PdfService : IPdfService
{
    public byte[] GenerateWristbandPdf(WristbandDto data)
    {
        // ─────────────────────────────────────────────────────────────────
        // QuestPDF - Wristband Design (Optimized for 25mm x 250mm or thermal)
        // ─────────────────────────────────────────────────────────────────
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                // Standard wristband size (approx 3.5" x 1")
                page.Size(280, 90);
                page.Margin(2);

                page.Content().Element(root =>
                {
                    root
                        .Border(2)
                        .BorderColor("#4099ff") // Professional blue border
                        .Background(Colors.White)
                        .Padding(4)
                        .Row(row =>
                        {
                            // Left: Patient Details
                            row.RelativeItem().Column(col =>
                            {
                                // Name in Parentheses
                                col.Item().Text($"({data.PatientName})")
                                    .FontSize(13)
                                    .ExtraBold()
                                    .FontColor(Colors.Black);

                                col.Item().PaddingTop(2);

                                // Medical Number
                                col.Item().Text(t =>
                                {
                                    t.Span($"{data.MedicalNumber}").FontSize(10).SemiBold();
                                    t.Span(" : الرقم الطبي").FontSize(10);
                                });

                                // Room Number
                                col.Item().Text(t =>
                                {
                                    t.Span($"{data.RoomNumber ?? "-"}").FontSize(11).Bold();
                                    t.Span(" : رقم الغرفة").FontSize(10);
                                });
                            });

                            // Right: QR Code
                            row.ConstantItem(65)
                                .AlignMiddle()
                                .AlignCenter()
                                .Image(data.QrCode, ImageScaling.FitArea);
                        });
                });
            });
        }).GeneratePdf();
    }
}