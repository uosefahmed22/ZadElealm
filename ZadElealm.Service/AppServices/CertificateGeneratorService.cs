using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.ServiceDto;
using ZadElealm.Core.Service;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace ZadElealm.Service.AppServices
{
    public class CertificateGeneratorService : ICertificateGeneratorService
    {
        private readonly string _certificatesPath;

        public CertificateGeneratorService()
        {
            _certificatesPath = "../ZadElealm.Apis/wwwroot/certificates";

            if (!Directory.Exists(_certificatesPath))
                Directory.CreateDirectory(_certificatesPath);

            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> GenerateCertificatePdf(CertificateDto certificateDto)
        {
            try
            {
                var fileName = $"certificate_{DateTime.Now.Ticks}.pdf";
                var outputFilePath = Path.Combine(_certificatesPath, fileName);

                var document = new CertificateDocument(certificateDto);
                document.GeneratePdf(outputFilePath);

                return $"/certificates/{fileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"حدث خطأ أثناء توليد الشهادة: {ex.Message}");
            }
        }
    }

    public class CertificateDocument : IDocument
    {
        private CertificateDto Certificate { get; }

        public CertificateDocument(CertificateDto certificate)
        {
            Certificate = certificate;
        }

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2.5f, Unit.Centimetre); // زيادة الهوامش
                    page.DefaultTextStyle(x => x.FontFamily("Arial"));

                    // المحتوى الرئيسي
                    page.Content()
                        .Column(x =>
                        {
                            x.Spacing(30); // زيادة المسافة بين العناصر

                            // بسملة
                            x.Item()
                                .AlignCenter()
                                .Text("بسم الله الرحمن الرحيم")
                                .FontSize(24) // زيادة حجم الخط
                                .FontColor(Colors.Grey.Darken2);

                            // عنوان الشهادة مع خط أسفله
                            x.Item().Column(title =>
                            {
                                title.Item()
                                    .AlignCenter()
                                    .Text("شهادة إتمام")
                                    .Bold()
                                    .FontSize(36) // زيادة حجم الخط
                                    .FontColor(Colors.Blue.Darken1); // تغيير اللون

                                title.Item()
                                    .LineHorizontal(1.5f) // زيادة سمك الخط
                                    .LineColor(Colors.Blue.Darken1);
                            });

                            // المقدمة
                            x.Item()
                                .AlignCenter()
                                .Text("الحمد لله رب العالمين والصلاة والسلام على أشرف المرسلين")
                                .FontSize(22) // زيادة حجم الخط
                                .FontColor(Colors.Grey.Darken2);

                            // النص الرئيسي في سطر واحد
                            x.Item()
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span(" قد أتم دراسة ")
                                        .FontSize(22) // زيادة حجم الخط
                                        .FontColor(Colors.Grey.Darken2);

                                    text.Span(Certificate.QuizName)
                                        .Bold()
                                        .FontSize(22) // زيادة حجم الخط
                                        .FontColor(Colors.Blue.Darken1); // تغيير اللون

                                    text.Span(" بفضل من الله وتوفيقه")
                                        .FontSize(22) // زيادة حجم الخط
                                        .FontColor(Colors.Grey.Darken2);
                                    text.Span(Certificate.UserName)
                                        .Bold()
                                        .FontSize(22) // زيادة حجم الخط
                                        .FontColor(Colors.Green.Darken2); // تغيير اللون

                                    text.Span(" نشهد أن الطالب الفاضل")
                                        .FontSize(22) // زيادة حجم الخط
                                        .FontColor(Colors.Grey.Darken2);


                                });

                            // الدعاء
                            x.Item()
                                .AlignCenter()
                                .Text("نسأل الله له دوام التوفيق والنجاح في الدنيا والآخرة")
                                .FontSize(22) // زيادة حجم الخط
                                .FontColor(Colors.Grey.Darken2);

                            // التاريخ
                            x.Item()
                                .AlignCenter()
                                .Text($"في يوم: {Certificate.CompletedDate:dd/MM/yyyy}")
                                .FontSize(22) // زيادة حجم الخط
                                .FontColor(Colors.Grey.Darken2);

                            // خط في النهاية
                            x.Item()
                                .LineHorizontal(1.5f) // زيادة سمك الخط
                                .LineColor(Colors.Black);
                        });
                });
        }
    }
}

