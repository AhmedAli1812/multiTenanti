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
                page.Size(200, 70);
                page.Margin(0);

                page.Content().ShowEntire().Element(root =>
                {
                    root
                        .Background(Colors.Blue.Lighten3)
                        .Padding(3)
                        .Border(2)
                        .BorderColor(Colors.Blue.Medium)
                        .Element(inner =>
                        {
                            inner
                                .Background(Colors.White)
                                .Padding(5)
                                .Row(row =>
                                {
                                    // ======================
                                    // 🧾 TEXT (Arabic Inline)
                                    // ======================
                                    row.RelativeItem().Column(col =>
                                    {
                                        // 👤 اسم المريض
                                        col.Item().Text(t =>
                                        {
                                            t.Span("اسم المريض: ")
                                                .SemiBold()
                                                .FontSize(10);

                                            t.Span(data.PatientName ?? "-")
                                                .Bold()
                                                .FontSize(10);
                                        });

                                        col.Item().PaddingTop(4);

                                        // 🆔 الرقم الطبي
                                        col.Item().Text(t =>
                                        {
                                            t.Span("الرقم الطبي: ")
                                                .SemiBold()
                                                .FontSize(9);

                                            t.Span(data.MedicalNumber ?? "-")
                                                .FontSize(9);
                                        });

                                        col.Item().PaddingTop(4);

                                        // 🏥 رقم الغرفة
                                        col.Item().Text(t =>
                                        {
                                            t.Span("رقم الغرفة: ")
                                                .SemiBold()
                                                .FontSize(9);

                                            t.Span(data.RoomNumber ?? "-")
                                                .Bold()
                                                .FontSize(11);
                                        });

                                        col.Item().PaddingTop(2);

                                        // 👇 رقم الغرفة كبير
                                        col.Item().Text(data.RoomNumber ?? "-")
                                            .FontSize(16)
                                            .Bold();
                                    });

                                    // ======================
                                    // 🔳 QR
                                    // ======================
                                    row.ConstantItem(50)
                                        .AlignMiddle()
                                        .AlignCenter()
                                        .Image(data.QrCode, ImageScaling.FitArea);
                                });
                        });
                });
            });
        }).GeneratePdf();
    }
}