using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using ZadElealm.Core.Models.Identity;
using MediatR;
using AdminDashboard.Commands;
using AdminDashboard.Quires;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using AdminDashboard.Dto;
using AdminDashboard.Helpers;
using ZadElealm.Core.Errors;
using AdminDashboard.Commands.AdminCommand;
using AdminDashboard.Quires.AdminQuery;

namespace AdminDashboard.Controllers
{
    namespace AdminDashboard.Controllers
    {
        public class AdminController : Controller
        {
            private readonly IMediator _mediator;
            private readonly UserManager<AppUser> _userManager;
            private readonly string _primaryAdminEmail;

            public AdminController(
                IMediator mediator,
                UserManager<AppUser> userManager,
                IOptions<AdminSettings> adminSettings)
            {
                _mediator = mediator;
                _userManager = userManager;
                _primaryAdminEmail = adminSettings.Value.PrimaryAdminEmail;
            }

            [HttpGet]
            [AllowAnonymous]
            public IActionResult Login()
            {
                if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
                    return RedirectToAction("Index", "Home");

                return View();
            }

            [HttpPost]
            [AllowAnonymous]
            [EnableRateLimiting("admin-login")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Login(LoginDTO model, CancellationToken cancellationToken)
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var command = new LoginCommand
                {
                    Email = model.Email,
                    Password = model.Password
                };

                var result = await _mediator.Send(command, cancellationToken);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.ErrorMessage);
                    return View(model);
                }

                await SetupUserAuthentication(result.User);
                return RedirectToAction("Index", "Home");
            }

            [HttpGet]
            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> AddAdmin(CancellationToken cancellationToken)
            {
                if (!IsPrimaryAdmin())
                    return RedirectToAction(nameof(AccessDenied));

                await UpdateStatsViewBag(cancellationToken);

                return View(new AdminDto());
            }

            [HttpPost]
            [Authorize(Roles = "Admin")] 
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> AddAdmin(AdminDto model, CancellationToken cancellationToken)
            {
                if (!IsPrimaryAdmin())
                    return RedirectToAction(nameof(AccessDenied));

                if (!ModelState.IsValid)
                {
                    await UpdateStatsViewBag(cancellationToken);
                    return View(model);
                }

                var email = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrWhiteSpace(email))
                    return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

                var command = new AddAdminCommand
                {
                    DisplayName = model.DisplayName,
                    Email = model.Email,
                    Password = model.Password
                };

                var result = await _mediator.Send(command, cancellationToken);

                if (result.StatusCode != 200)
                {
                    ModelState.AddModelError("", result.Message ?? "An error occurred");
                    await UpdateStatsViewBag(cancellationToken);
                    return View(model);
                }

                TempData["SuccessMessage"] = result.Message ?? "Administrator added successfully";
                return RedirectToAction("Index", "User");
            }

            [HttpPost]
            [Authorize]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Logout()
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login");
            }

            [HttpGet]
            [Authorize]
            public IActionResult AccessDenied() => View();

            private bool IsPrimaryAdmin()
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                return !string.IsNullOrWhiteSpace(_primaryAdminEmail) &&
                       string.Equals(email, _primaryAdminEmail, StringComparison.OrdinalIgnoreCase);
            }

            private async Task UpdateStatsViewBag(CancellationToken cancellationToken)
            {
                var stats = await _mediator.Send(new GetAdminStatsQuery(), cancellationToken);
                ViewBag.AdminCount = stats.CurrentAdminCount;
                ViewBag.MaxAdminCount = stats.MaxAdminCount;
            }
            private async Task SetupUserAuthentication(AppUser user)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
                };

                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
            }
        }
    }
}
