using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZadElealm.Apis.Commands.VideoProgressCommands;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Quaries.VideoProgressQueries;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Apis.Controllers
{
    public class VideoProgressController : ApiBaseController
    {
        private readonly IMediator _mediator;
        private readonly UserManager<AppUser> _userManager;

        public VideoProgressController(IMediator mediator,UserManager<AppUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("update")]
        public async Task<ActionResult<VideoProgressDto>> UpdateProgress([FromBody] UpdateProgressRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) {
                return BadRequest(new ApiResponse(400, "User not found"));
            }

            var command = new UpdateVideoProgressCommand
            {
                UserId = user.Id,
                VideoId = request.VideoId,
                WatchedDuration = TimeSpan.FromSeconds(request.WatchedSeconds)
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<CourseProgressDto>> GetCourseProgress(int courseId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(new ApiResponse(400, "User not found"));
            }

            var query = new GetCourseProgressQuery
            {
                UserId = user.Id,
                CourseId = courseId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet("check-eligibility/{courseId}")]
        public async Task<ActionResult<EligibilityResponse>> CheckQuizEligibility(int courseId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(new ApiResponse(400, "User not found"));
            }

            var query = new CheckQuizEligibilityQuery
            {
                UserId = user.Id,
                CourseId = courseId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet("video/{videoId}")]
        public async Task<ActionResult<VideoProgressDto>> GetVideoProgress(int videoId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(new ApiResponse(400, "User not found"));
            }

            var query = new GetVideoProgressQuery
            {
                UserId = user.Id,
                VideoId = videoId
            };

            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound(new ApiResponse(404, "Video progress not found"));

            return Ok(result);
        }
    }
}
