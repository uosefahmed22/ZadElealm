using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.ServiceDto;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using ZadElealm.Service.AppServices;

namespace ZadElealm.Apis.Controllers
{
    public class CertificateController : ApiBaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICertificateService _certificateService;

        public CertificateController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, ICertificateService certificateService)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _certificateService = certificateService;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<CertificateDto>> GenerateCertificate(string userId, int quizId, int score)
        {
            try
            {
                var certificate = await _certificateService.GenerateCertificate(userId, quizId, score);
                return Ok(certificate);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadCertificate(int id)
        {
            try
            {
                var spec = new CertificateByIdSpecification(id);
                var certificate = await _unitOfWork.Repository<Certificate>()
                    .GetEntityWithSpecAsync(spec);

                if (certificate == null)
                    return NotFound(new { message = "الشهادة غير موجودة" });

                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, certificate.ImageUrl.TrimStart('/'));

                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { message = "ملف الشهادة غير موجود" });

                var memory = await System.IO.File.ReadAllBytesAsync(filePath);
                return File(memory, "application/pdf", $"certificate_{certificate.Name}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IReadOnlyList<CertificateDto>>> GetUserCertificates(string userId)
        {
            try
            {
                var spec = new CertificatesByUserSpecification(userId);
                var certificates = await _unitOfWork.Repository<Certificate>()
                    .GetAllWithSpecAsync(spec);

                var certificateDtos = certificates.Select(c => new CertificateDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    CompletedDate = c.Compliateddate,
                    UserName = c.User.UserName,
                    QuizName = c.Quiz.Name
                }).ToList();

                return Ok(certificateDtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
