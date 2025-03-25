using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZadElealm.Core.Models.Identity;
using MediatR;
using AdminDashboard.Commands;
using AdminDashboard.Quires;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using AdminDashboard.Dto;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Controllers
{
    namespace AdminDashboard.Controllers
    {
        public class AdminController : Controller
        {
            private readonly IMediator _mediator;
            private readonly SignInManager<AppUser> _signInManager;
            private readonly UserManager<AppUser> _userManager;
            private const string BaseEmail = "uosefahmed0022@gmail.com";

            public AdminController(
                IMediator mediator,
                SignInManager<AppUser> signInManager,
                UserManager<AppUser> userManager)
            {
                _mediator = mediator;
                _signInManager = signInManager;
                _userManager = userManager;
            }

            [HttpGet]
            public IActionResult Login()
            {
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Login(LoginDTO model)
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

                var result = await _mediator.Send(command);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.ErrorMessage);
                    return View(model);
                }

                await SetupUserAuthentication(result.User);
                return RedirectToAction("Index", "Home");
            }

            public async Task<IActionResult> AddAdmin()
            {
                var stats = await _mediator.Send(new GetAdminStatsQuery());

                return View(new AdminDto());
            }

            [HttpPost]
            [Authorize(Roles = "Admin")] 
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> AddAdmin(AdminDto model)
            {
                if (!ModelState.IsValid)
                {
                    var stats = await _mediator.Send(new GetAdminStatsQuery());
                    ViewBag.AdminCount = stats.CurrentAdminCount;
                    ViewBag.MaxAdminCount = stats.MaxAdminCount;
                    return View(model);
                }

                var email = User.FindFirstValue(ClaimTypes.Email);

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

                if (email != BaseEmail)
                {
                    return BadRequest(new ApiResponse(400, "You are not authorized to perform this action."));
                }

                var command = new AddAdminCommand
                {
                    DisplayName = model.DisplayName,
                    Email = model.Email,
                    Password = model.Password
                };

                var result = await _mediator.Send(command);

                if (result.StatusCode != 200)
                {
                    ModelState.AddModelError("", result.Message ?? "An error occurred");
                    await UpdateStatsViewBag();
                    return View(model);
                }

                TempData["SuccessMessage"] = result.Message ?? "Administrator added successfully";
                return RedirectToAction("Index", "User");
            }

            public async Task<IActionResult> Logout()
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login");
            }

            private async Task UpdateStatsViewBag()
            {
                var stats = await _mediator.Send(new GetAdminStatsQuery());
                ViewBag.AdminCount = stats.CurrentAdminCount;
                ViewBag.MaxAdminCount = stats.MaxAdminCount;
            }
            private async Task SetupUserAuthentication(AppUser user)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await _signInManager.SignInAsync(user, isPersistent: false);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
            }
        }
    }
}