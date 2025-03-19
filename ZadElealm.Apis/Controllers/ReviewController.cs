using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ZadElealm.Apis.Commands.Review;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Review;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using static System.Net.Mime.MediaTypeNames;

namespace ZadElealm.Apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ApiBaseController
    {
        private readonly IMediator _mediator;
        private readonly UserManager<AppUser> _userManager;

        public ReviewController(IMediator mediator, UserManager<AppUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddReview([FromBody] ReviewDto request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            var command = new AddReviewCommand(request.ReviewText, request.CourseId, user.Id);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("{reviewId}/replies")]
        public async Task<ActionResult<ApiResponse>> AddReply(int reviewId, [FromBody] string replyText)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            var command = new AddReplyCommand
            {
                ReviewId = reviewId,
                ReplyText = replyText,
                UserId =user.Id
            };

            return Ok(await _mediator.Send(command));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("{reviewId}/like")]
        public async Task<ActionResult<ApiResponse>> ToggleLike(int reviewId) 
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            var command = new ToggleLikeCommand
            {
                ReviewId = reviewId,
                UserId = user.Id
            };

            return Ok(await _mediator.Send(command));
        }

        [HttpGet("{reviewId}/replies")]
        public async Task<ActionResult<ApiResponse>> GetReplies(int reviewId)
        {
            var query = new GetReviewRepliesQuery(reviewId);
            var response = await _mediator.Send(query);

            return StatusCode(response.StatusCode, response);
        }
    }
}