using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ZadElealm.Apis.Commands.Rating;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using ZadElealm.Apis.Quaries.Review;

namespace ZadElealm.Apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ApiBaseController
    {
        private readonly IMediator _mediator;
        private readonly UserManager<AppUser> _userManager;

        public RatingController(IMediator mediator, UserManager<AppUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddRating([FromBody] RatingDto request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new AddRatingCommand(request.Value, request.CourseId, user.Id);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet("can-rate/{courseId}")]
        public async Task<ActionResult<ApiResponse>> CanRate(int courseId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var canRate = await _mediator.Send(new GetUserAddRateingBeforeQuery(courseId, user.Id));
            return Ok(new ApiDataResponse(200, canRate));
        }
    }
}
