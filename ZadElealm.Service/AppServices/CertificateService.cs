using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Models.ServiceDto;
using ZadElealm.Core.Models;
using ZadElealm.Core.Service;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;
using Microsoft.AspNetCore.Identity;
using ZadElealm.Core.Specifications.Certificate;
using ZadElealm.Core.Specifications.Quiz;

namespace ZadElealm.Service.AppServices
{
    public class CertificateService : ICertificateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICertificateGeneratorService _certificateGenerator;
        private readonly UserManager<AppUser> _userManager;

        public CertificateService(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            ICertificateGeneratorService certificateGenerator)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _certificateGenerator = certificateGenerator;
        }

        public async Task<CertificateDto> GenerateCertificate(string userId, int quizId)
        {
            try
            {
                var progressSpec = new ProgressWithCompletedQuizSpecification(userId, quizId);
                var progress = await _unitOfWork.Repository<Progress>()
                    .GetEntityWithSpecAsync(progressSpec);

                if (progress == null || !progress.IsCompleted)
                    throw new Exception("لم يتم اجتياز الاختبار بعد");

                var certificateSpec = new CertificateByUserAndQuizSpecification(userId, quizId);
                var existingCertificate = await _unitOfWork.Repository<Certificate>()
                    .GetEntityWithSpecAsync(certificateSpec);

                if (existingCertificate != null)
                    throw new Exception("تم إصدار الشهادة مسبقاً");

                var user = await _userManager.FindByIdAsync(userId);
                var quizSpec = new QuizByIdSpecification(quizId);
                var quiz = await _unitOfWork.Repository<Quiz>()
                    .GetEntityWithSpecAsync(quizSpec);

                if (user == null || quiz == null)
                    throw new Exception("بيانات المستخدم أو الاختبار غير صحيحة");

                var certificate = new Certificate
                {
                    Name = $"شهادة اجتياز {quiz.Name}",
                    Description = $"تم منح هذه الشهادة لإتمام {quiz.Name} بنجاح",
                    CreatedAt = DateTime.Now,
                    UserId = userId,
                    QuizId = quizId
                };

                var certificateDto = new CertificateDto
                {
                    Name = certificate.Name,
                    Description = certificate.Description,
                    CompletedDate = certificate.CreatedAt,
                    UserName = user.UserName,
                    QuizName = quiz.Name
                };

                var pdfPath = await _certificateGenerator.GenerateCertificatePdf(certificateDto);
                certificate.PdfUrl = pdfPath;

                await _unitOfWork.Repository<Certificate>().AddAsync(certificate);
                await _unitOfWork.Complete();

                certificateDto.PdfUrl = pdfPath;
                return certificateDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
