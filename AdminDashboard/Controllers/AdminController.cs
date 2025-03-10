using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AdminController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

            // Check if user is in Admin role
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                ModelState.AddModelError("", "Access denied.");
                return View(model);
            }

            // If everything is successful
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AddAdmin()
        {
            return View(new AdminDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdmin(AdminDto model)
        {
            if (!ModelState.IsValid)
            {
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
                ModelState.AddModelError("", "Email already exists.");
                return View(model);
            }

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                user.EmailConfirmed = true;
                TempData["SuccessMessage"] = "Admin added successfully";
                return RedirectToAction("Index", "User");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
    }
}
