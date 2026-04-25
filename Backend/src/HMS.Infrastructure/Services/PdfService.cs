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
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                // 🔥 FIX النهائي (بدون Unit)
                page.Size(90, 25);
                page.Margin(2);

                page.Content().Row(row =>
                {
                    // 🔳 QR
                    row.ConstantItem(35).Height(35).Image(data.QrCode);

                    // 🧾 Text
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(data.PatientName)
                            .FontSize(8)
                            .Bold();

                        col.Item().Text($"MRN: {data.MedicalNumber}")
                            .FontSize(7);
                    });
                });
            });
        }).GeneratePdf();
    }
}