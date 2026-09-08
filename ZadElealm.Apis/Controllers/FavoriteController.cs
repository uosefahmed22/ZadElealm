using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZadElealm.Apis.Commands.FavoriteCommand;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Quaries.Favorite;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using ZadElealm.Repository.Repositories;

namespace ZadElealm.Apis.Controllers
{
    public class FavoriteController : ApiBaseController
    {
        private readonly IMediator _mediator;
        private readonly UserManager<AppUser> _userManager;

        public FavoriteController(IMediator mediator,UserManager<AppUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetFavorites()
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new ApiResponse(401, "المستخد غير موجود"));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var query = new GetFavoriteCoursesQuery(user.Id);
            var response = await _mediator.Send(query);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("{courseId}")]
        public async Task<ActionResult<ApiResponse>> AddToFavorites(int courseId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new AddFavoriteCourseCommand(user.Id, courseId);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpDelete("{courseId}")]
        public async Task<ActionResult<ApiResponse>> RemoveFromFavorites(int courseId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new RemoveFavoriteCourseCommand(user.Id, courseId);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }
    }
}
