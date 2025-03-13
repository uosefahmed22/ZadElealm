using AdminDashboard.Dto;
using AdminDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;

namespace AdminDashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly string _primaryAdminEmail;
        private readonly int _maxAdminCount;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _primaryAdminEmail = configuration["AdminSettings:PrimaryAdminEmail"];
            _maxAdminCount = int.Parse(configuration["AdminSettings:MaxAdminCount"] ?? "10");
        }

        [HttpPost]
        public async Task<IActionResult> Create(RoleFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var confirmedAdmin = await _userManager.FindByEmailAsync(_primaryAdminEmail);
                if (confirmedAdmin != null)
                {
                    if (!confirmedAdmin.EmailConfirmed)
                    {
                        TempData["ErrorMessage"] = "لا يمكن إضافة أدوار للنظام حتى يتم تأكيد البريد الإلكتروني للمدير الأساسي.";
                        return RedirectToAction("Index", "User");
                    }
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null || currentUser.Email != _primaryAdminEmail)
                {
                    TempData["ErrorMessage"] = "غير مسموح لك بإضافة أدوار للنظام.";
                    return RedirectToAction("AccessDenied", "Account");
                }

                var rolesCount = await _roleManager.Roles.CountAsync();
                if (rolesCount >= _maxAdminCount)
                {
                    TempData["ErrorMessage"] = $"لا يمكن إضافة المزيد من الأدوار. الحد الأقصى هو {_maxAdminCount} أدوار.";
                    return RedirectToAction(nameof(Index));
                }

                var roleExists = await _roleManager.RoleExistsAsync(model.Name);
                if (!roleExists)
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Name));
                    TempData["SuccessMessage"] = "تم إنشاء الدور بنجاح.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("Name", "الدور موجود بالفعل!");
                    return View("Index", await _roleManager.Roles.ToListAsync());
                }
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Email != _primaryAdminEmail)
            {
                TempData["ErrorMessage"] = "غير مسموح لك بإدارة الأدوار في النظام.";
                return RedirectToAction("AccessDenied", "Account");
            }
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role != null && (role.Name == "Admin" || role.Name == "User"))
            {
                ModelState.AddModelError("Name", "Cannot delete this role!");
                return View("Index", await _roleManager.Roles.ToListAsync());
            }

            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            var mappedRole = new RoleViewModel()
            {
                Name = role.Name
            };

            return View(mappedRole);
        }

        [HttpPost]
        public async Task<IActionResult> Update(string id, RoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.RoleExistsAsync(model.Name);
                if (!role)
                {
                    var roleToUpdate = await _roleManager.FindByIdAsync(id);
                    roleToUpdate.Name = model.Name;
                    await _roleManager.UpdateAsync(roleToUpdate);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("Name", "Role already exists!");
                    return View("Index", await _roleManager.Roles.ToListAsync());
                }
            }
                return RedirectToAction(nameof(Index));
        }
    }
}
