using CloudinaryDotNet;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZadElealm.Apis.Commands.UserRankCommand;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.UserRankquery;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Apis.Controllers
{
    public class RankController : ApiBaseController
    {
        private readonly IMediator _mediator;
        private readonly UserManager<AppUser> _userManager;

        public RankController(IMediator mediator,UserManager<AppUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [HttpGet("user")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [ProducesResponseType(typeof(UserRankDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserRankDto>> GetUserRank()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var query = new GetUserRankQuery { UserId = user.Id };
            var result = await _mediator.Send(query);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("top")]
        [ProducesResponseType(typeof(List<UserRankDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserRankDto>>> GetTopRankedUsers([FromQuery] int take = 10)
        {
            var query = new GetTopRankedUsersQuery { Take = take };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("update")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UpdateUserRank()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new UpdateUserRankCommand { UserId = user.Id };
            var result = await _mediator.Send(command);

            if (!result) return BadRequest("Failed to update user rank");
            return Ok();
        }

        [HttpGet("calculate")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> CalculateUserPoints()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));
            var command = new CalculateUserPointsCommand { UserId = user.Id };
            var result = await _mediator.Send(command);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("leaderboard")]
        [ProducesResponseType(typeof(List<UserRankDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserRankDto>>> GetLeaderboard([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetTopRankedUsersQuery { Take = pageSize };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(Dictionary<UserRankEnum, int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Dictionary<UserRankEnum, int>>> GetRankStats()
        {
            var stats = new Dictionary<UserRankEnum, int>
            {
                { UserRankEnum.Bronze, 0 },
                { UserRankEnum.Silver, 0 },
                { UserRankEnum.Gold, 0 },
                { UserRankEnum.Platinum, 0 },
                { UserRankEnum.Diamond, 0 }
            };
            return Ok(stats);
        }
    }
}
