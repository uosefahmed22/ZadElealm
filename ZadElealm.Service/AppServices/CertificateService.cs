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
using ZadElealm.Apis.Errors;


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

        public async Task<ApiDataResponse> GenerateAndSaveCertificate(string userId, int quizId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                var quiz = await _unitOfWork.Repository<Quiz>().GetEntityAsync(quizId);

                var progressSpec = new ProgressWithUserDataAndQuiz(userId, quizId);
                var progress = await _unitOfWork.Repository<Progress>().GetEntityWithSpecAsync(progressSpec);

                if (progress == null)
                    return new ApiDataResponse(404, null, "لم يتم العثور على الاختبار");

                if (!progress.IsCompleted)
                    return new ApiDataResponse(400, null, "لم يتم اجتياز الاختبار بعد");


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

                return new ApiDataResponse(200, certificate, "تم إنشاء الشهادة بنجاح");
            }
            catch (Exception ex)
            {
                throw new Exception($"فشل في إنشاء الشهادة: {ex.Message}");
            }
        }
        private (string filePath, string fileName) GeneratePdfCertificate(string userId, int quizId, AppUser user, Quiz quiz)
        {
            var goldColor = Color.FromRGB(218, 165, 32);
            var greyDark = Color.FromRGB(96, 96, 96);
            var blueDark = Color.FromRGB(25, 25, 112);
            var redDark = Color.FromRGB(139, 0, 0);

            QuestPDF.Settings.License = LicenseType.Community;

            var certificatesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "certificates");
            if (!Directory.Exists(certificatesDirectory))
            {
                Directory.CreateDirectory(certificatesDirectory);
            }

            var fileName = $"certificate_{userId}_{quizId}_{Guid.NewGuid()}.pdf";
            var filePath = Path.Combine(certificatesDirectory, fileName);
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "certificates", "logo.png");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontFamily("Arial"));

                    page.Background()
                        .Border(2)
                        .BorderColor(goldColor)
                        .Padding(5)
                        .Border(1)
                        .BorderColor(greyDark);


                    page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.ConstantItem(80);

                            row.RelativeItem().AlignCenter()
                                .PaddingTop(10)
                                .Text("بسم الله الرحمن الرحيم")
                                .FontSize(24)
                                .FontColor(goldColor);

                            row.ConstantItem(80).AlignRight()
                                .Image(logoPath, ImageScaling.FitArea);
                        });

                        column.Item().Height(8);

                        column.Item().AlignCenter()
                            .Text("شهادة تقدير")
                            .FontSize(36)
                            .FontColor(goldColor)
                            .Bold();

                        column.Item().Height(3);

                        column.Item().Height(2)
                            .Background(goldColor)
                            .ExtendHorizontal();

                        column.Item().Height(20);

                        column.Item().AlignCenter()
                            .Text(text =>
                            {
                                text.Span("تشهد")
                                                        .FontSize(20)
                                                        .FontColor(greyDark);

                                text.EmptyLine();
                                text.Span("أكاديمية زاد العلم")
                                   .FontSize(24)
                                   .Bold()
                                   .FontColor(blueDark);

                                text.EmptyLine();
                                text.Span("بأن الطالب")
                                   .FontSize(20)
                                   .FontColor(greyDark);

                                text.EmptyLine();
                                text.Span(user.DisplayName)
                                   .FontSize(36)
                                   .Bold()
                                   .FontColor(redDark);

                                text.EmptyLine();
                                text.Span("قد اجتاز بنجاح اختبار")
                                   .FontSize(20)
                                   .FontColor(greyDark);

                                text.EmptyLine();
                                text.Span(quiz.Name)
                                   .FontSize(28)
                                   .Bold()
                                   .FontColor(blueDark);
                            });

                        column.Item().Height(20);

                        column.Item()
                            .Row(row =>
                            {
                                row.RelativeItem().AlignCenter()
                                    .Text(text =>
                                    {
                                        text.Span("أكاديمية زاد العلم")
                                            .FontSize(18)
                                            .FontColor(blueDark)
                                            .Bold();
                                        text.EmptyLine();
                                        text.Span("____________")
                                            .FontSize(18)
                                            .FontColor(greyDark);
                                    });

                                row.RelativeItem().AlignCenter()
                                    .Text(text =>
                                    {
                                        text.Span("تاريخ الإصدار")
                                            .FontSize(18)
                                            .FontColor(blueDark)
                                            .Bold();
                                        text.EmptyLine();
                                        text.Span(DateTime.Now.ToString("dd/MM/yyyy"))
                                            .FontSize(18)
                                            .FontColor(greyDark);
                                    });
                            });

                        column.Item().Height(10);

                        column.Item().AlignCenter()
                            .Text($"رقم الشهادة: {Guid.NewGuid().ToString().Substring(0, 8)}")
                            .FontSize(14)
                            .FontColor(greyDark);
                    });
                });
            })
                           .GeneratePdf(filePath);

            if (!File.Exists(filePath))
            {
                throw new Exception("Failed to generate PDF file");
            }

            return (filePath, fileName);
        }
    }
}