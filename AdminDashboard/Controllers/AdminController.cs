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
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly string _primaryAdminEmail;
        private readonly int _maxAdminCount;

        public AdminController(UserManager<AppUser> userManager,
            IConfiguration configuration,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _primaryAdminEmail = configuration["AdminSettings:PrimaryAdminEmail"];
            _maxAdminCount = int.Parse(configuration["AdminSettings:MaxAdminCount"] ?? "10");

        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await _signInManager.SignInAsync(user, isPersistent: false);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return RedirectToAction("Index", "Home");
        }
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> AddAdmin()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Email != _primaryAdminEmail)
            {
                TempData["ErrorMessage"] = "غير مسموح لك بإضافة مديرين للنظام.";
                return RedirectToAction("AccessDenied", "Account");
            }

            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            if (adminUsers.Count >= _maxAdminCount)
            {
                TempData["ErrorMessage"] = $"لا يمكن إضافة أكثر من {_maxAdminCount} مديرين للنظام.";
                return RedirectToAction("Index", "User");
            }

            ViewBag.AdminCount = adminUsers.Count;
            ViewBag.MaxAdminCount = _maxAdminCount;

            return View(new AdminDto());
        }
        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<IActionResult> AddAdmin(AdminDto model)
        {
            if (!ModelState.IsValid)
            {
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                ViewBag.AdminCount = adminUsers.Count;
                ViewBag.MaxAdminCount = _maxAdminCount;
                return View(model);
            }
            var confimedAdmin = await _userManager.FindByEmailAsync(_primaryAdminEmail);
            if (confimedAdmin != null) {
                if (!confimedAdmin.EmailConfirmed)
                {
                    TempData["ErrorMessage"] = "لا يمكن إضافة مديرين للنظام حتى يتم تأكيد البريد الإلكتروني للمدير الأساسي.";
                    return RedirectToAction("Index", "User");
                }
            }
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Email != _primaryAdminEmail)
            {
                ModelState.AddModelError("", "غير مسموح لك بإضافة مديرين للنظام.");
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                ViewBag.AdminCount = adminUsers.Count;
                ViewBag.MaxAdminCount = _maxAdminCount;
                return View(model);
            }

            var adminCount = await _userManager.GetUsersInRoleAsync("Admin");
            if (adminCount.Count >= _maxAdminCount)
            {
                ModelState.AddModelError("", $"لا يمكن إضافة أكثر من {_maxAdminCount} مديرين للنظام.");
                ViewBag.AdminCount = adminCount.Count;
                ViewBag.MaxAdminCount = _maxAdminCount;
                return View(model);
            }

            var user = new AppUser
            {
                DisplayName = model.DisplayName,
                UserName = model.Email,
                Email = model.Email
            };

            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError("", "البريد الإلكتروني مستخدم بالفعل.");
                ViewBag.AdminCount = adminCount.Count;
                ViewBag.MaxAdminCount = _maxAdminCount;
                return View(model);
            }

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);

                TempData["SuccessMessage"] = "تمت إضافة المدير بنجاح";
                return RedirectToAction("Index", "User");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.AdminCount = adminCount.Count;
            ViewBag.MaxAdminCount = _maxAdminCount;
            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
    }
}
