using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZadElealm.Apis.Commands.FavoriteCommand;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
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

        public FavoriteController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = new GetFavoriteCoursesQuery(userId);
            var response = await _mediator.Send(query);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("{courseId}")]
        public async Task<ActionResult<ApiResponse>> AddToFavorites(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var command = new AddFavoriteCourseCommand(userId, courseId);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpDelete("{courseId}")]
        public async Task<ActionResult<ApiResponse>> RemoveFromFavorites(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var command = new RemoveFavoriteCourseCommand(userId, courseId);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }
    }
}
