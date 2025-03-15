using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Controllers
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using System.Security.Claims;
    using ZadElealm.Apis.Dtos.Auth;
    using ZadElealm.Core.Models.Identity;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System.Threading.Tasks;
    using MediatR;
    using global::AdminDashboard.Commands;
    using global::AdminDashboard.Quires;

    namespace AdminDashboard.Controllers
    {
        public class AdminController : Controller
        {
            private readonly IMediator _mediator;
            private readonly SignInManager<AppUser> _signInManager;
            private readonly UserManager<AppUser> _userManager;
            private readonly ILogger<AdminController> _logger;

            public AdminController(
                IMediator mediator,
                SignInManager<AppUser> signInManager,
                UserManager<AppUser> userManager,  
                ILogger<AdminController> logger)
            {
                _mediator = mediator;
                _signInManager = signInManager;
                _userManager = userManager;
                _logger = logger;
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
                try
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
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during login");
                    ModelState.AddModelError("", "An error occurred during login.");
                    return View(model);
                }
            }

            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> AddAdmin()
            {
                try
                {
                    var stats = await _mediator.Send(new GetAdminStatsQuery());
                    ViewBag.AdminCount = stats.CurrentAdminCount;
                    ViewBag.MaxAdminCount = stats.MaxAdminCount;

                    return View(new AdminDto());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting admin stats");
                    TempData["ErrorMessage"] = "An error occurred while loading the page.";
                    return RedirectToAction("Index", "Home");
                }
            }

            [HttpPost]
            [Authorize(Roles = "Admin")]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> AddAdmin(AdminDto model)
            {
                try
                {
                    if (!ModelState.IsValid)
                    {
                        var stats = await _mediator.Send(new GetAdminStatsQuery());
                        ViewBag.AdminCount = stats.CurrentAdminCount;
                        ViewBag.MaxAdminCount = stats.MaxAdminCount;
                        return View(model);
                    }

                    var command = new AddAdminCommand
                    {
                        DisplayName = model.DisplayName,
                        Email = model.Email,
                        Password = model.Password
                    };

                    var result = await _mediator.Send(command);

                    if (!result.Succeeded)
                    {
                        if (result.Errors != null)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", result.ErrorMessage);
                        }

                        var stats = await _mediator.Send(new GetAdminStatsQuery());
                        ViewBag.AdminCount = stats.CurrentAdminCount;
                        ViewBag.MaxAdminCount = stats.MaxAdminCount;
                        return View(model);
                    }

                    TempData["SuccessMessage"] = "Administrator added successfully";
                    return RedirectToAction("Index", "User");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error adding admin");
                    ModelState.AddModelError("", "An error occurred while adding the administrator.");
                    return View(model);
                }
            }

            public async Task<IActionResult> Logout()
            {
                await _mediator.Send(new LogoutCommand());
                return RedirectToAction(nameof(Login));
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