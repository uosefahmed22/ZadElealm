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
        public async Task<ActionResult<ApiResponse>> AddReview([FromBody] AddReviewRequest request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            var command = new AddReviewCommand(request.ReviewText, request.CourseId, user.Id);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }
    }
}