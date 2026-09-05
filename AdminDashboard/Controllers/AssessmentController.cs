using AdminDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Assessment;

namespace AdminDashboard.Controllers;

[Authorize(Roles = "Admin")]
public sealed class AssessmentController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public AssessmentController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var assessments = await _unitOfWork.Repository<Assessment>()
            .GetAllWithSpecNoTrackingAsync(new AssessmentDashboardSpecification());
        return View(new AssessmentDashboardIndexViewModel
        {
            Assessments = assessments.Select(assessment => new AssessmentDashboardItemViewModel
            {
                Id = assessment.Id,
                Name = assessment.Name,
                Description = assessment.Description,
                CategoryName = assessment.Category.Name,
                PassingScore = assessment.PassingScore,
                IsActive = assessment.IsActive,
                FormCount = assessment.Forms.Count,
                QuestionCount = assessment.Forms.Sum(form => form.Questions.Count)
            }).ToList()
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var assessment = await _unitOfWork.Repository<Assessment>()
            .GetEntityWithSpecNoTrackingAsync(new AssessmentWithFormsSpecification(id, false));
        if (assessment == null)
            return NotFound();

        return View(new AssessmentDashboardDetailsViewModel
        {
            Id = assessment.Id,
            Name = assessment.Name,
            Description = assessment.Description,
            CategoryName = assessment.Category.Name,
            PassingScore = assessment.PassingScore,
            IsActive = assessment.IsActive,
            Forms = assessment.Forms.OrderBy(form => form.InternalCode).Select(form =>
                new AssessmentFormAdminViewModel
                {
                    Id = form.Id,
                    InternalCode = form.InternalCode,
                    IsActive = form.IsActive,
                    Questions = form.Questions.OrderBy(question => question.Id).Select(question =>
                        new AssessmentQuestionAdminViewModel
                        {
                            Id = question.Id,
                            Text = question.Text,
                            Choices = question.Choices.OrderBy(choice => choice.Id).Select(choice =>
                                new AssessmentChoiceAdminViewModel
                                {
                                    Text = choice.Text,
                                    IsCorrect = choice.IsCorrect
                                }).ToList()
                        }).ToList()
                }).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var assessment = await _unitOfWork.Repository<Assessment>().GetEntityAsync(id);
        if (assessment == null)
            return NotFound();

        assessment.IsActive = !assessment.IsActive;
        _unitOfWork.Repository<Assessment>().Update(assessment);
        await _unitOfWork.Complete();
        TempData["Success"] = assessment.IsActive
            ? "تم تفعيل الاختبار للطلاب."
            : "تم إيقاف الاختبار عن الطلاب.";
        return RedirectToAction(nameof(Index));
    }
}
