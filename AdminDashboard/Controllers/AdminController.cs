using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Core.Models.Identity;
using MediatR;
using AdminDashboard.Commands;
using AdminDashboard.Quires;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace AdminDashboard.Controllers
{
    namespace AdminDashboard.Controllers
    {
        public class AdminController : Controller
        {
            private readonly IMediator _mediator;
            private readonly SignInManager<AppUser> _signInManager;
            private readonly UserManager<AppUser> _userManager;

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

            public async Task<IActionResult> AddAdmin()
            {
                try
                {
                    var stats = await _mediator.Send(new GetAdminStatsQuery());
                    ViewBag.AdminCount = stats.CurrentAdminCount;
                    ViewBag.MaxAdminCount = stats.MaxAdminCount;

                    if (ViewBag.AdminCount == null) ViewBag.AdminCount = 0;
                    if (ViewBag.MaxAdminCount == null) ViewBag.MaxAdminCount = 1;

                    return View(new AdminDto());
                }
                catch (Exception ex)
                {
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
                    ModelState.AddModelError("", "An error occurred while adding the administrator.");
                    return View(model);
                }
            }

            public async Task<IActionResult> Logout()
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Login");
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