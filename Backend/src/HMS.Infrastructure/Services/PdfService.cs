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
                // ✅ حجم أكبر عشان الـ content يتناسب
                page.Size(250, 100);
                page.Margin(0);

                page.Content().Element(root =>
                {
                    root
                        .Background(Colors.Blue.Lighten3)
                        .Padding(3)
                        .Element(inner =>
                        {
                            inner
                                .Background(Colors.White)
                                .Padding(6)
                                .Row(row =>
                                {
                                    // ======================
                                    // 🧾 TEXT
                                    // ======================
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text(t =>
                                        {
                                            t.Span("اسم المريض: ")
                                                .SemiBold()
                                                .FontSize(9);
                                            t.Span(data.PatientName ?? "-")
                                                .Bold()
                                                .FontSize(9);
                                        });

                                        col.Item().PaddingTop(3);

                                        col.Item().Text(t =>
                                        {
                                            t.Span("الرقم الطبي: ")
                                                .SemiBold()
                                                .FontSize(8);
                                            t.Span(data.MedicalNumber ?? "-")
                                                .FontSize(8);
                                        });

                                        col.Item().PaddingTop(3);

                                        col.Item().Text(t =>
                                        {
                                            t.Span("رقم الغرفة: ")
                                                .SemiBold()
                                                .FontSize(8);
                                            t.Span(data.RoomNumber ?? "-")
                                                .Bold()
                                                .FontSize(12);
                                        });
                                    });

                                    // ======================
                                    // 🔳 QR
                                    // ======================
                                    row.ConstantItem(70)
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