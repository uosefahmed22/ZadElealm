using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Quaries.Certificate;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using ZadElealm.Service.AppServices;

namespace ZadElealm.Apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificateController : ApiBaseController
    {
        private readonly IMediator _mediator;
        private readonly UserManager<AppUser> _userManager;

        public CertificateController(IMediator mediator, UserManager<AppUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet("user")]
        public async Task<ActionResult<ApiResponse>> GetUserCertificates()
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401));

            var query = new GetUserCertificatesQuery(user.Id);
            var response = await _mediator.Send(query);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet("{certificateId:int}/file")]
        public async Task<IActionResult> GetCertificateFile(
            int certificateId,
            CancellationToken cancellationToken)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401));

            var query = new GetCertificateFileQuery(certificateId, user.Id);
            var response = await _mediator.Send(query, cancellationToken);
            if (response.StatusCode != StatusCodes.Status200OK)
                return StatusCode(response.StatusCode, response);

            if (response is not ApiDataResponse { Data: CertificateFileDownloadDto file })
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ApiResponse(500, "تعذر قراءة ملف الشهادة"));

            Response.Headers.CacheControl = "private, no-store";
            Response.Headers.Pragma = "no-cache";
            return new FileContentResult(file.Content, "application/pdf")
            {
                EnableRangeProcessing = true
            };
        }
    }
}
