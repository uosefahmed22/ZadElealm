using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZadElealm.Apis.Commands.ReplyCommand;
using ZadElealm.Apis.Commands.Review;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Review;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;

namespace ZadElealm.Apis.Controllers
{
    public class ReplyController : ApiBaseController
    {
        private readonly IMediator _mediator;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        public ReplyController(IMediator mediator, 
            UserManager<AppUser> userManager,   
            IMapper mapper)
        {
            _mediator = mediator;
            _userManager = userManager;
            _mapper = mapper;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("likeReply/{replyId}")]
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
        public async Task<ActionResult<ApiResponse>> DeleteReply(int replyId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new DeleteReplyreviewCommand(replyId, user.Id);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpGet("{reviewId}/replies")]
        public async Task<ActionResult<ApiResponse>> GetReplies(int reviewId)
        {
            var query = new GetReviewRepliesQuery(reviewId);
            var response = await _mediator.Send(query);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("addReply/{reviewId}")]
        public async Task<ActionResult<ApiResponse>> AddReply(int reviewId, [FromBody] string replyText)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new AddReplyCommand
            {
                ReviewId = reviewId,
                ReplyText = replyText,
                UserId = user.Id
            };
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }
    }
}
