using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZadElealm.Apis.Commands.ReplyCommand;
using ZadElealm.Apis.Commands.Review;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Quaries.Review;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using ZadElealm.Apis.Dtos;

namespace ZadElealm.Apis.Controllers
{
    public class ReplyController : ApiBaseController
    {
        private readonly IMediator _mediator;
        private readonly UserManager<AppUser> _userManager;
        public ReplyController(IMediator mediator, 
            UserManager<AppUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("likeReply/{replyId}")]
        [HttpPost("{replyId}/like")]
        public async Task<IActionResult> LikeReply(int replyId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new LikeReplyCommand(replyId, user.Id);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpDelete("{replyId}/reply")]
        [HttpDelete("{replyId}")]
        public async Task<ActionResult<ApiResponse>> DeleteReply(int replyId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new DeleteReplyreviewCommand(replyId, user.Id);
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet("{reviewId}/replies")]
        [HttpGet("review/{reviewId}")]
        public async Task<ActionResult<ApiResponse>> GetReplies(int reviewId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var query = new GetReviewRepliesQuery(reviewId, user.Id);
            var response = await _mediator.Send(query);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("addReply/{reviewId}")]
        [HttpPost("review/{reviewId}")]
        public async Task<ActionResult<ApiResponse>> AddReply(int reviewId, [FromBody] ReplyRequestDto request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new AddReplyCommand
            {
                ReviewId = reviewId,
                ReplyText = request.ReplyText,
                UserId = user.Id
            };
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }
    }
}
