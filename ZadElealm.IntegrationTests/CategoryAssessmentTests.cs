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
        var firstQuestions = firstData.GetProperty("questions");
        var secondQuestions = secondData.GetProperty("questions");
        Assert.Equal(25, firstQuestions.GetArrayLength());
        Assert.Equal(30, firstData.GetProperty("durationMinutes").GetInt32());
        Assert.Equal(
            firstData.GetProperty("attemptExpiresAtUtc").GetDateTime(),
            secondData.GetProperty("attemptExpiresAtUtc").GetDateTime());
        var firstQuestion = firstQuestions[0];
        var secondQuestion = secondQuestions[0];
        Assert.Equal(firstQuestion.GetProperty("id").GetInt32(), secondQuestion.GetProperty("id").GetInt32());
        Assert.False(firstData.TryGetProperty("forms", out _));
        Assert.False(firstData.TryGetProperty("formId", out _));

        var submission = new AssessmentSubmissionDto
        {
            StudentAnswers = firstQuestions.EnumerateArray()
                .Select(question => new StudentAnswerDto
                {
                    QuestionId = question.GetProperty("id").GetInt32(),
                    ChoiceId = question.GetProperty("choices")
                        .EnumerateArray()
                        .Single(choice => choice.GetProperty("text").GetString() == "الإجابة الصحيحة")
                        .GetProperty("id").GetInt32()
                }).ToList()
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

    [Fact]
    public async Task Student_with_eighty_percent_progress_cannot_open_the_assessment()
    {
        var assessmentId = await SeedAssessmentAtEightyPercentAsync();
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _factory.GenerateToken("User"));

        var response = await client.GetAsync($"/api/Assessment/{assessmentId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Contains(
            "أكمل جميع دروس دورة واحدة",
            await response.Content.ReadAsStringAsync());
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
            DurationMinutes = 30,
            Category = category,
            Forms = Enumerable.Range(1, 5).Select(index => new AssessmentForm
            {
                InternalCode = $"FIQH-{index}",
                Questions = Enumerable.Range(1, 25).Select(questionIndex =>
                    new AssessmentQuestion
                    {
                        Text = $"سؤال النموذج {index} رقم {questionIndex}",
                        DisplayOrder = questionIndex,
                        Difficulty = questionIndex <= 10
                            ? AssessmentQuestionDifficulty.Easy
                            : questionIndex <= 20
                                ? AssessmentQuestionDifficulty.Medium
                                : AssessmentQuestionDifficulty.Hard,
                        Choices =
                        [
                            new AssessmentChoice { Text = "الإجابة الصحيحة", IsCorrect = true },
                            new AssessmentChoice { Text = "إجابة غير صحيحة", IsCorrect = false }
                        ]
                    }).ToList()
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

    private async Task<int> SeedAssessmentAtEightyPercentAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync("user@test.com")
            ?? throw new InvalidOperationException("Integration test user was not seeded.");
        var suffix = Guid.NewGuid().ToString("N");
        var category = new Category
        {
            Name = $"تصنيف-{suffix}",
            Description = "تصنيف لاختبار شرط اكتمال الدورة",
            ImageUrl = "https://example.test/category.jpg",
            Courses = []
        };
        var course = new Course
        {
            Name = $"دورة-{suffix}",
            Description = "دورة مكتملة بنسبة ثمانين بالمائة",
            Author = "مدرس الاختبار",
            CourseLanguage = "العربية",
            CourseVideosCount = 5,
            ImageUrl = "https://example.test/course.jpg",
            Category = category,
            Videos = Enumerable.Range(1, 5).Select(index => new Video
            {
                Title = $"الدرس {index}",
                Description = "درس تجريبي",
                VideoUrl = $"https://example.test/video-{index}",
                ThumbnailUrl = "https://example.test/video.jpg",
                VideoDuration = TimeSpan.FromMinutes(10)
            }).ToList()
        };
        var assessment = new Assessment
        {
            Name = $"اختبار-{suffix}",
            Description = "اختبار شرط اكتمال الدورة",
            PassingScore = 60,
            DurationMinutes = 30,
            Category = category,
            Forms =
            [
                new AssessmentForm
                {
                    InternalCode = $"FORM-{suffix}",
                    Questions =
                    [
                        new AssessmentQuestion
                        {
                            Text = "سؤال تجريبي",
                            DisplayOrder = 1,
                            Difficulty = AssessmentQuestionDifficulty.Easy,
                            Choices =
                            [
                                new AssessmentChoice { Text = "صحيح", IsCorrect = true },
                                new AssessmentChoice { Text = "خطأ", IsCorrect = false }
                            ]
                        }
                    ]
                }
            ]
        };

        dbContext.AddRange(course, assessment);
        await dbContext.SaveChangesAsync();
        dbContext.Enrollments.Add(new Enrollment { CourseId = course.Id, AppUserId = user.Id });
        dbContext.VideoProgresses.AddRange(course.Videos.Take(4).Select(video => new VideoProgress
        {
            UserId = user.Id,
            CourseId = course.Id,
            VideoId = video.Id,
            WatchedDuration = video.VideoDuration,
            IsCompleted = true
        }));
        await dbContext.SaveChangesAsync();
        return assessment.Id;
    }
}
