using AdminDashboard.Commands;
using AdminDashboard.Commands.RoleCommand;
using AdminDashboard.Models;
using AdminDashboard.Quires;
using AdminDashboard.Quires.AdminQuery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZadElealm.Core.Models.Identity;

[Authorize(Roles = "Admin")]
public class RoleController : Controller
{
    private readonly IMediator _mediator;
    private readonly UserManager<AppUser> _userManager;
    private readonly string _primaryAdminEmail;

    public RoleController(
        IMediator mediator,
        UserManager<AppUser> userManager,
        IConfiguration configuration)
    {
        _mediator = mediator;
        _userManager = userManager;
        _primaryAdminEmail = configuration["AdminSettings:PrimaryAdminEmail"];
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null || currentUser.Email != _primaryAdminEmail)
        {
            TempData["ErrorMessage"] = "غير مسموح لك بإدارة الأدوار في النظام.";
            return RedirectToAction("AccessDenied", "Account");
        }

        var roles = await _mediator.Send(new GetRolesQuery());
        return View(roles);
    }

    [HttpPost]
    public async Task<IActionResult> Create(RoleFormViewModel model)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Index));

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null || currentUser.Email != _primaryAdminEmail)
        {
            TempData["ErrorMessage"] = "غير مسموح لك بإضافة أدوار للنظام.";
            return RedirectToAction("AccessDenied", "Admin");
        }

        var result = await _mediator.Send(new CreateRoleCommand { Name = model.Name });

        if (result.StatusCode == 200)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(string id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null || currentUser.Email != _primaryAdminEmail)
        {
            TempData["ErrorMessage"] = "غير مسموح لك بحذف الأدوار من النظام.";
            return RedirectToAction("AccessDenied", "Account");
        }

        var result = await _mediator.Send(new DeleteRoleCommand { Id = id });

        if (result.StatusCode == 200)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Update(string id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null || currentUser.Email != _primaryAdminEmail)
        {
            TempData["ErrorMessage"] = "غير مسموح لك بتعديل الأدوار في النظام.";
            return RedirectToAction("AccessDenied", "Account");
        }

        var role = await _mediator.Send(new GetRoleByIdQuery { Id = id });
        if (role == null)
        {
            TempData["ErrorMessage"] = "الدور غير موجود!";
            return RedirectToAction(nameof(Index));
        }

        var mappedRole = new RoleViewModel
        {
            Name = role.Name
        };

        return View(mappedRole);
    }

    [HttpPost]
    public async Task<IActionResult> Update(string id, RoleViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null || currentUser.Email != _primaryAdminEmail)
        {
            TempData["ErrorMessage"] = "غير مسموح لك بتعديل الأدوار في النظام.";
            return RedirectToAction("AccessDenied", "Account");
        }

        var result = await _mediator.Send(new UpdateRoleCommand
        {
            Id = id,
            Name = model.Name
        });

        if (result.StatusCode == 200)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["ErrorMessage"] = result.Message;
        return View(model);
    }
}