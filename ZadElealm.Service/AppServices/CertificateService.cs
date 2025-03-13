using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Service;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Models.Identity;
using Microsoft.AspNetCore.Identity;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZadElealm.Core.Specifications.CertificateFolder;
using ZadElealm.Core.Models;
using ZadElealm.Core.Specifications;


namespace ZadElealm.Service.AppServices
{
    public class CertificateService : ICertificateService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public CertificateService(
            UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<Certificate> GenerateAndSaveCertificate(string userId, int quizId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                var quiz = await _unitOfWork.Repository<Quiz>().GetEntityAsync(quizId);

                if (user == null || quiz == null)
                    throw new Exception("المستخدم أو الاختبار غير موجود");

                var progressSpec = new ProgressWithUserDataAndQuiz(userId, quizId);
                var progress = await _unitOfWork.Repository<Progress>().GetEntityWithSpecAsync(progressSpec);

                if (progress == null)
                    throw new Exception("لم يتم العثور على محاولة للاختبار");

                if (!progress.IsCompleted)
                    throw new Exception("لم يتم إكمال الاختبار بعد");

               
                var (filePath, fileName) = GeneratePdfCertificate(userId, quizId, user, quiz);

                var baseUrl = _configuration["BaseUrl"];
                var pdfUrl = $"{baseUrl}/certificates/{fileName}";

                var certificate = new Certificate
                {
                    Name = $"Certificate_{user.DisplayName}_{quiz.Name}",
                    Description = $"Certificate for completing {quiz.Name} with score {progress.Score}",  // Added score to description
                    PdfUrl = pdfUrl,
                    UserId = userId,
                    QuizId = quizId,
                    CreatedAt = DateTime.UtcNow
                };

                return certificate;
            }
            catch (Exception ex)
            {
                throw new Exception($"فشل في إنشاء الشهادة: {ex.Message}");
            }
        }
        private (string filePath, string fileName) GeneratePdfCertificate(string userId, int quizId, AppUser user, Quiz quiz)
        {
            // Initialize QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            // Setup directory
            var certificatesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "certificates");
            if (!Directory.Exists(certificatesDirectory))
            {
                Directory.CreateDirectory(certificatesDirectory);
            }

            // Create file paths
            var fileName = $"certificate_{userId}_{quizId}_{Guid.NewGuid()}.pdf";
            var filePath = Path.Combine(certificatesDirectory, fileName);

            // Generate PDF
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontFamily("Arial"));

                    // Background with border
                    page.Background()
                        .Border(2)
                        .BorderColor(Colors.Grey.Darken1)
                        .Padding(15)
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten1)
                        .Padding(25);

                    page.Content()
                        .PaddingVertical(50)
                        .Column(column =>
                        {
                            // Title - شهادة تقدير
                            column.Item()
                                .Height(60)
                                .AlignCenter()
                                .Text("شهادة تقدير", TextStyle.Default
                                    .FontSize(48)
                                    .FontColor(Colors.Blue.Medium)
                                    .Bold());

                            // Spacing after title
                            column.Item().Height(40);

                            // Content
                            column.Item().AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("نشهد بأن الطالب", TextStyle.Default
                                        .FontSize(32)
                                        .FontColor(Colors.Black));

                                    text.EmptyLine();
                                    text.EmptyLine();

                                    text.Span(user.DisplayName, TextStyle.Default
                                        .FontSize(42)
                                        .Bold()
                                        .FontColor(Colors.Red.Medium));

                                    text.EmptyLine();
                                    text.EmptyLine();

                                    text.Span("قد اجتاز بنجاح امتحان", TextStyle.Default
                                        .FontSize(32)
                                        .FontColor(Colors.Black));

                                    text.EmptyLine();
                                    text.EmptyLine();

                                    text.Span(quiz.Name, TextStyle.Default
                                        .FontSize(38)
                                        .Bold()
                                        .FontColor(Colors.Blue.Medium));

                                    text.EmptyLine();
                                    text.EmptyLine();

                                    text.Span("ونتمنى له دوام التوفيق والنجاح", TextStyle.Default
                                        .FontSize(32)
                                        .FontColor(Colors.Black));
                                });

                            // Spacing before date
                            column.Item().Height(100);

                            // Date
                            column.Item().AlignCenter()
                                .Text(text =>
                                {
                                    text.Span("تحريراً في: ", TextStyle.Default
                                        .FontSize(24)
                                        .FontColor(Colors.Grey.Darken3));
                                    text.Span(DateTime.Now.ToString("dd/MM/yyyy"), TextStyle.Default
                                        .FontSize(24)
                                        .FontColor(Colors.Grey.Darken3));
                                });
                        });
                });
            })
            .GeneratePdf(filePath);

            // Verify file creation
            if (!File.Exists(filePath))
            {
                throw new Exception("Failed to generate PDF file");
            }

            return (filePath, fileName);
        }

    }
}
