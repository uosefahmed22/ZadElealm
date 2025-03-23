using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZadElealm.Apis.Commands.Notification;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Notification;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;
using ZadElealm.Repository.Repositories;

namespace ZadElealm.Apis.Controllers
{
    public class NotificationController : ApiBaseController
    {
        private readonly IMediator _mediator;
        private readonly UserManager<AppUser> _userManager;

        public NotificationController(IMediator mediator, UserManager<AppUser> userManager)
        {
            _mediator = mediator;
            _userManager = userManager;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetNotifications()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var query = new GetUserNotificationsQuery(user.Id);
            var response = await _mediator.Send(query);

            return StatusCode(response.StatusCode, response);
        }
       
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("mark-all-as-read")]
        public async Task<ActionResult<ApiResponse>> MarkAllAsRead()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new MarkAllNotificationsAsReadCommand(user.Id);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("mark-as-read/{notificationId}")]
        public async Task<ActionResult<ApiResponse>> MarkAsRead(int notificationId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new MarkNotificationAsReadCommand(user.Id, notificationId);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpDelete("{notificationId}")]
        public async Task<ActionResult<ApiResponse>> DeleteNotification(int notificationId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new DeleteNotificationCommand(user.Id, notificationId);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet("{notificationId}")]
        public async Task<ActionResult<ApiResponse>> GetNotificationById(int notificationId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var query = new GetNotificationByIdQuery(user.Id, notificationId);
            var response = await _mediator.Send(query);

            return StatusCode(response.StatusCode, response);
        }
    }
}
