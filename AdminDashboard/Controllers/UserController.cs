using AdminDashboard.Commands.UserCommand;
using AdminDashboard.Models;
using AdminDashboard.Quires.UserQuery;
using MediatR;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using AdminDashboard.Helpers;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IMediator _mediator;
        private readonly string _primaryAdminEmail;

        public UserController(IMediator mediator, IOptions<AdminSettings> adminSettings)
        {
            _mediator = mediator;
            _primaryAdminEmail = adminSettings.Value.PrimaryAdminEmail;
        }
        public async Task<IActionResult> Index()
        {
            var users = await _mediator.Send(new GetAllUsersQuery());
            if (users == null)
            {
                TempData["ErrorMessage"] = "No users found!";
                return View(new List<UserViewModel>());
            }
            return View(users);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!IsPrimaryAdmin())
                return RedirectToAction("AccessDenied", "Admin");

            var query = new GetUserForEditQuery { UserId = id };
            var viewModel = await _mediator.Send(query);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserRolesViewModel model)
        {
            if (!IsPrimaryAdmin())
                return RedirectToAction("AccessDenied", "Admin");

            var command = new UpdateUserRolesCommand { Model = model };
            var result = await _mediator.Send(command);

            if (!result.StatusCode.Equals(200))
            {
                ModelState.AddModelError(string.Empty, "Failed to update user roles!");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var command = new DeleteUserCommand { UserId = id };
            var result = await _mediator.Send(command);

            if (!result.StatusCode.Equals(200))
            {
                TempData["ErrorMessage"] = result.Message;
            }
            var users = await _mediator.Send(new GetAllUsersQuery());
            return View("Index", users);
        }

        private bool IsPrimaryAdmin()
            => !string.IsNullOrWhiteSpace(_primaryAdminEmail) &&
               string.Equals(
                   User.FindFirstValue(ClaimTypes.Email),
                   _primaryAdminEmail,
                   StringComparison.OrdinalIgnoreCase);
    }
}
