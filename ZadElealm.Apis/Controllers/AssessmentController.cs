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
public sealed class AssessmentController : ApiBaseController
{
    private readonly IAssessmentService _assessmentService;
    private readonly UserManager<AppUser> _userManager;

    public AssessmentController(
        IAssessmentService assessmentService,
        UserManager<AppUser> userManager)
    {
        _assessmentService = assessmentService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse>> GetAvailable()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

        var response = await _assessmentService.GetAvailableAssessmentsAsync(user.Id);
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{assessmentId:int}")]
    public async Task<ActionResult<ApiResponse>> GetAssessment(int assessmentId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

        var response = await _assessmentService.GetAssessmentAsync(user.Id, assessmentId);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("{assessmentId:int}/submit")]
    public async Task<ActionResult<ApiResponse>> Submit(
        int assessmentId,
        [FromBody] AssessmentSubmissionDto submission)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

        var response = await _assessmentService
            .SubmitAssessmentAsync(user.Id, assessmentId, submission);
        return StatusCode(response.StatusCode, response);
    }

    private async Task<AppUser?> GetCurrentUserAsync()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        return string.IsNullOrWhiteSpace(email) ? null : await _userManager.FindByEmailAsync(email);
    }
}
