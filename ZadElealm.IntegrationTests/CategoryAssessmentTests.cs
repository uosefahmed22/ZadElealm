using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Repository.Data.Datbases;
using Xunit;

namespace ZadElealm.IntegrationTests;

public sealed class CategoryAssessmentTests : IClassFixture<ZadElealmApiFactory>
{
    private readonly ZadElealmApiFactory _factory;

    public CategoryAssessmentTests(ZadElealmApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Eligible_student_sees_one_public_assessment_and_can_earn_certificate()
    {
        var assessmentId = await SeedEligibleFiqhAssessmentAsync();
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _factory.GenerateToken("User"));

        var listResponse = await client.GetAsync("/api/Assessment");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listJson = await listResponse.Content.ReadAsStringAsync();
        Assert.Contains("اختبار الفقه", listJson);
        Assert.DoesNotContain("FIQH-", listJson);
        Assert.DoesNotContain("internalCode", listJson, StringComparison.OrdinalIgnoreCase);

        var firstGet = await client.GetAsync($"/api/Assessment/{assessmentId}");
        var secondGet = await client.GetAsync($"/api/Assessment/{assessmentId}");
        Assert.Equal(HttpStatusCode.OK, firstGet.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondGet.StatusCode);

        using var firstDocument = JsonDocument.Parse(await firstGet.Content.ReadAsStringAsync());
        using var secondDocument = JsonDocument.Parse(await secondGet.Content.ReadAsStringAsync());
        var firstData = firstDocument.RootElement.GetProperty("data");
        var secondData = secondDocument.RootElement.GetProperty("data");
        var firstQuestion = firstData.GetProperty("questions")[0];
        var secondQuestion = secondData.GetProperty("questions")[0];
        Assert.Equal(firstQuestion.GetProperty("id").GetInt32(), secondQuestion.GetProperty("id").GetInt32());
        Assert.False(firstData.TryGetProperty("forms", out _));
        Assert.False(firstData.TryGetProperty("formId", out _));

        var correctChoice = firstQuestion.GetProperty("choices")
            .EnumerateArray()
            .Single(choice => choice.GetProperty("text").GetString() == "الإجابة الصحيحة");
        var submission = new AssessmentSubmissionDto
        {
            StudentAnswers =
            [
                new StudentAnswerDto
                {
                    QuestionId = firstQuestion.GetProperty("id").GetInt32(),
                    ChoiceId = correctChoice.GetProperty("id").GetInt32()
                }
            ]
        };

        var submitResponse = await client.PostAsJsonAsync(
            $"/api/Assessment/{assessmentId}/submit",
            submission);
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);
        var submitJson = await submitResponse.Content.ReadAsStringAsync();
        Assert.Contains("\"isCompleted\":true", submitJson);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.True(await dbContext.Certificates.AnyAsync(certificate =>
            certificate.AssessmentId == assessmentId));
    }

    private async Task<int> SeedEligibleFiqhAssessmentAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync("user@test.com")
            ?? throw new InvalidOperationException("Integration test user was not seeded.");

        var existing = await dbContext.Assessments.FirstOrDefaultAsync(x => x.Name == "اختبار الفقه");
        if (existing != null)
            return existing.Id;

        var category = new Category
        {
            Name = "الفقه الإسلامي",
            Description = "اختبارات الفقه",
            ImageUrl = "https://example.test/fiqh.jpg",
            Courses = []
        };
        var course = new Course
        {
            Name = "مدخل الفقه",
            Description = "دورة مؤهلة لاختبار الفقه",
            Author = "مدرس الاختبار",
            CourseLanguage = "العربية",
            CourseVideosCount = 1,
            ImageUrl = "https://example.test/fiqh-course.jpg",
            Category = category,
            Videos =
            [
                new Video
                {
                    Title = "درس الفقه",
                    Description = "درس تجريبي",
                    VideoUrl = "https://example.test/video",
                    ThumbnailUrl = "https://example.test/video.jpg",
                    VideoDuration = TimeSpan.FromMinutes(10)
                }
            ]
        };
        var assessment = new Assessment
        {
            Name = "اختبار الفقه",
            Description = "اختبار واحد للطالب بخمسة نماذج داخلية.",
            PassingScore = 60,
            Category = category,
            Forms = Enumerable.Range(1, 5).Select(index => new AssessmentForm
            {
                InternalCode = $"FIQH-{index}",
                Questions =
                [
                    new AssessmentQuestion
                    {
                        Text = $"سؤال النموذج {index}",
                        Choices =
                        [
                            new AssessmentChoice { Text = "الإجابة الصحيحة", IsCorrect = true },
                            new AssessmentChoice { Text = "إجابة غير صحيحة", IsCorrect = false }
                        ]
                    }
                ]
            }).ToList()
        };

        dbContext.AddRange(course, assessment);
        await dbContext.SaveChangesAsync();
        dbContext.Enrollments.Add(new Enrollment { CourseId = course.Id, AppUserId = user.Id });
        dbContext.VideoProgresses.Add(new VideoProgress
        {
            UserId = user.Id,
            CourseId = course.Id,
            VideoId = course.Videos.Single().Id,
            WatchedDuration = TimeSpan.FromMinutes(10),
            IsCompleted = true
        });
        await dbContext.SaveChangesAsync();
        return assessment.Id;
    }
}
