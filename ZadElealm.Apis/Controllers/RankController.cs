using CloudinaryDotNet;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using ZadElealm.Apis.Commands.UserRankCommand;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Errors;
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
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var query = new GetUserRankQuery { UserId = user.Id };
            var result = await _mediator.Send(query);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("dashboard")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [ProducesResponseType(typeof(ApiDataResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiDataResponse>> GetRankDashboard(
            [FromQuery, Range(3, 50)] int take = 10)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new ApiResponse(401, "بيانات الدخول غير مكتملة"));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var result = await _mediator.Send(new GetRankDashboardQuery
            {
                UserId = user.Id,
                Take = take
            });

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("top")]
        [ProducesResponseType(typeof(List<UserRankDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserRankDto>>> GetTopRankedUsers(
            [FromQuery, Range(1, 100)] int take = 10)
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
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new UpdateUserRankCommand { UserId = user.Id };
            var result = await _mediator.Send(command);

            if (!result) return BadRequest("Failed to update user rank");
            return Ok();
        }

        [HttpGet("calculate")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> CalculateUserPoints()
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));
            var command = new CalculateUserPointsCommand { UserId = user.Id };
            var result = await _mediator.Send(command);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("leaderboard")]
        [ProducesResponseType(typeof(List<UserRankDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<UserRankDto>>> GetLeaderboard(
            [FromQuery, Range(1, 1_000_000)] int page = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10)
        {
            var query = new GetTopRankedUsersQuery
            {
                Skip = (page - 1) * pageSize,
                Take = pageSize
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(Dictionary<UserRankEnum, int>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Dictionary<UserRankEnum, int>>> GetRankStats()
        {
            var stats = await _mediator.Send(new GetRankStatsQuery());
            return Ok(stats);
        }
    }
}
