using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;
using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Apis.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
public sealed class AchievementsController : ApiBaseController
{
    private readonly IAchievementService _achievementService;
    private readonly UserManager<AppUser> _userManager;

    public AchievementsController(
        IAchievementService achievementService,
        UserManager<AppUser> userManager)
    {
        _achievementService = achievementService;
        _userManager = userManager;
    }

    [HttpGet("mine")]
    [ProducesResponseType(typeof(ApiDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiDataResponse>> GetMine(CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

        var dashboard = await _achievementService.GetMineAsync(user.Id, cancellationToken);
        return Ok(new ApiDataResponse(200, dashboard));
    }

    [HttpPost("check-in")]
    [ProducesResponseType(typeof(ApiDataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiDataResponse>> CheckIn(CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

        var dashboard = await _achievementService.CheckInAsync(user.Id, cancellationToken);
        return Ok(new ApiDataResponse(200, dashboard));
    }

    private async Task<AppUser?> GetCurrentUserAsync()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        return string.IsNullOrWhiteSpace(email)
            ? null
            : await _userManager.FindByEmailAsync(email);
    }
}
