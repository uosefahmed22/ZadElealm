using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using ZadElealm.Apis.Commands.EnrollmentCommands;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.EnrollmentQuery;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentController : ApiBaseController
    {
        private readonly IMediator _mediator;
        private readonly UserManager<AppUser> _userManager;

        public EnrollmentController(
            IMediator mediator,
            UserManager<AppUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("{courseId}")]
        public async Task<ActionResult<ApiResponse>> EnrollCourse(int courseId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) 
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new EnrollCourseCommand(courseId, user.Id);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpDelete("{courseId}")]
        public async Task<ActionResult<ApiResponse>> UnenrollCourse(int courseId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new UnenrollCourseCommand(courseId, user.Id);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetEnrolledCourses()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var query = new GetEnrolledCoursesQuery(user.Id);
            var response = await _mediator.Send(query);

            return StatusCode(response.StatusCode, response);
        }
    }
}
