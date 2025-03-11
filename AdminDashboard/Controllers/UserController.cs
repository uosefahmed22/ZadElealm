using AdminDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<IActionResult> Index()
        {
            var Users = await _userManager.Users.Select(u => new UserViewModel
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                IsDeleted = u.IsDeleted,
                DisplayName = u.DisplayName,
                Roles = _userManager.GetRolesAsync(u).Result
            }).ToListAsync();

            return View(Users);
        }
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var AllRoles = await _roleManager.Roles.ToListAsync();
            var viewModel = new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.DisplayName,
                IsDeleted = user.IsDeleted,
                Roles = AllRoles.Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    IsSelected = _userManager.IsInRoleAsync(user, r.Name).Result
                }).ToList(),
            };
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(UserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            user.DisplayName = model.UserName;
            user.IsDeleted = model.IsDeleted;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in model.Roles)
            {
                if (userRoles.Any(r => r == role.Name) && !role.IsSelected)
                {
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                if (!userRoles.Any(r => r == role.Name) && role.IsSelected)
                {
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                ModelState.AddModelError("Name", "User not found!");
                return View("Index", await _userManager.Users.ToListAsync());
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
            {
                ModelState.AddModelError("Name", "Cannot delete a user with the Admin role!");
                return View("Index", await _userManager.Users.ToListAsync());
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Index", await _userManager.Users.ToListAsync());
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
