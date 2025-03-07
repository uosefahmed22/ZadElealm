using DinkToPdf.Contracts;
using DinkToPdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.ServiceDto;
using ZadElealm.Core.Service;

namespace ZadElealm.Service.AppServices
{
    public class CertificateGeneratorService : ICertificateGeneratorService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IConverter _converter;

        public CertificateGeneratorService(
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            IConverter converter)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _converter = converter;
        }

        public async Task<string> GenerateCertificatePdf(CertificateDto certificateDto)
        {
            try
            {
                var templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "templates", "certificates", "certificate-template.html");
                var cssPath = Path.Combine(_webHostEnvironment.WebRootPath, "templates", "certificates", "certificate-style.css");

                var html = await File.ReadAllTextAsync(templatePath);
                var css = await File.ReadAllTextAsync(cssPath);

                html = html.Replace("{UserName}", certificateDto.UserName)
                           .Replace("{QuizName}", certificateDto.QuizName)
                           .Replace("{CompletedDate}", certificateDto.CompletedDate.ToString("dd/MM/yyyy"));

                var fullHtml = html.Replace("</head>", $"<style>{css}</style></head>");

                var fileName = $"certificate_{DateTime.Now.Ticks}.pdf";
                var outputPath = Path.Combine(_webHostEnvironment.WebRootPath, "certificates", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Landscape,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                },
                    Objects = {
                    new ObjectSettings {
                        PagesCount = true,
                        HtmlContent = fullHtml,
                        WebSettings = {
                            DefaultEncoding = "UTF-8",
                            EnableIntelligentShrinking = true
                        },
                        HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                        FooterSettings = { FontSize = 9, Line = true, Center = "Certificate" }
                    }
                }
                };

                var pdfBytes = _converter.Convert(doc);
                await File.WriteAllBytesAsync(outputPath, pdfBytes);

                return $"/certificates/{fileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"حدث خطأ أثناء توليد الشهادة: {ex.Message}");
            }
        }
    }
}
