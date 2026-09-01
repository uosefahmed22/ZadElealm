using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.IntegrationTests;

public class LearningJourneyTests : IClassFixture<ZadElealmApiFactory>
{
    private readonly ZadElealmApiFactory _factory;

    public LearningJourneyTests(ZadElealmApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Student_CompletesLearningJourney_InOrder_AndReceivesCertificate()
    {
        var seeded = await SeedLearningJourneyAsync();
        using var client = CreateAuthenticatedClient();

        var progressWithoutEnrollment = await UpdateProgressAsync(client, seeded.FirstVideoId, 85);
        Assert.Equal(HttpStatusCode.Forbidden, progressWithoutEnrollment.StatusCode);

        var previewResponse = await client.GetAsync($"/api/Course/{seeded.CourseId}");
        Assert.Equal(HttpStatusCode.OK, previewResponse.StatusCode);
        using (var preview = JsonDocument.Parse(await previewResponse.Content.ReadAsStringAsync()))
        {
            var previewData = preview.RootElement.GetProperty("data");
            Assert.False(previewData.GetProperty("isEnrolled").GetBoolean());
            Assert.All(
                previewData.GetProperty("videos").EnumerateArray(),
                video => Assert.Equal(string.Empty, video.GetProperty("videoUrl").GetString()));
        }

        var enrollmentResponse = await client.PostAsync($"/api/Enrollment/{seeded.CourseId}", null);
        Assert.Equal(HttpStatusCode.OK, enrollmentResponse.StatusCode);

        var duplicateEnrollmentResponse = await client.PostAsync($"/api/Enrollment/{seeded.CourseId}", null);
        Assert.Equal(HttpStatusCode.BadRequest, duplicateEnrollmentResponse.StatusCode);

        var courseResponse = await client.GetAsync($"/api/Course/{seeded.CourseId}");
        Assert.Equal(HttpStatusCode.OK, courseResponse.StatusCode);
        using (var course = JsonDocument.Parse(await courseResponse.Content.ReadAsStringAsync()))
        {
            var courseData = course.RootElement.GetProperty("data");
            Assert.True(courseData.GetProperty("isEnrolled").GetBoolean());
            Assert.Equal(2, courseData.GetProperty("videos").GetArrayLength());
            Assert.All(
                courseData.GetProperty("videos").EnumerateArray(),
                video => Assert.False(string.IsNullOrWhiteSpace(video.GetProperty("videoUrl").GetString())));
        }

        var outOfOrderResponse = await UpdateProgressAsync(client, seeded.SecondVideoId, 85);
        Assert.Equal(HttpStatusCode.Forbidden, outOfOrderResponse.StatusCode);

        var lockedQuizResponse = await client.GetAsync($"/api/Quiz/{seeded.QuizId}");
        Assert.Equal(HttpStatusCode.Forbidden, lockedQuizResponse.StatusCode);

        var firstVideoResponse = await UpdateProgressAsync(client, seeded.FirstVideoId, 85);
        Assert.Equal(HttpStatusCode.OK, firstVideoResponse.StatusCode);

        var secondVideoResponse = await UpdateProgressAsync(client, seeded.SecondVideoId, 85);
        Assert.Equal(HttpStatusCode.OK, secondVideoResponse.StatusCode);

        var progressResponse = await client.GetAsync($"/api/VideoProgress/course/{seeded.CourseId}");
        Assert.Equal(HttpStatusCode.OK, progressResponse.StatusCode);
        using (var progress = JsonDocument.Parse(await progressResponse.Content.ReadAsStringAsync()))
        {
            Assert.Equal(100, progress.RootElement.GetProperty("overallProgress").GetSingle());
            Assert.True(progress.RootElement.GetProperty("isEligibleForQuiz").GetBoolean());
        }

        var quizResponse = await client.GetAsync($"/api/Quiz/{seeded.QuizId}");
        Assert.Equal(HttpStatusCode.OK, quizResponse.StatusCode);
        var answer = await ReadFirstQuizAnswerAsync(quizResponse);

        var submissionResponse = await client.PostAsJsonAsync("/api/Quiz/submit", new
        {
            quizId = seeded.QuizId,
            studentAnswers = new[]
            {
                new { questionId = answer.QuestionId, choiceId = answer.ChoiceId }
            }
        });
        Assert.Equal(HttpStatusCode.OK, submissionResponse.StatusCode);
        using (var submission = JsonDocument.Parse(await submissionResponse.Content.ReadAsStringAsync()))
        {
            Assert.Equal(100, submission.RootElement.GetProperty("data").GetProperty("score").GetInt32());
            Assert.True(submission.RootElement.GetProperty("data").GetProperty("isCompleted").GetBoolean());
        }

        var duplicateSubmissionResponse = await client.PostAsJsonAsync("/api/Quiz/submit", new
        {
            quizId = seeded.QuizId,
            studentAnswers = new[]
            {
                new { questionId = answer.QuestionId, choiceId = answer.ChoiceId }
            }
        });
        Assert.Equal(HttpStatusCode.BadRequest, duplicateSubmissionResponse.StatusCode);

        var certificatesResponse = await client.GetAsync("/api/Certificate/user");
        Assert.Equal(HttpStatusCode.OK, certificatesResponse.StatusCode);
        using var certificates = JsonDocument.Parse(await certificatesResponse.Content.ReadAsStringAsync());
        var certificateItems = certificates.RootElement.GetProperty("data");
        Assert.Single(certificateItems.EnumerateArray());
        Assert.Equal(
            "https://example.test/certificates/integration-test.pdf",
            certificateItems[0].GetProperty("pdfUrl").GetString());
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _factory.GenerateToken("User"));
        return client;
    }

    private static Task<HttpResponseMessage> UpdateProgressAsync(
        HttpClient client,
        int videoId,
        int watchedSeconds)
    {
        return client.PostAsJsonAsync("/api/VideoProgress/update", new { videoId, watchedSeconds });
    }

    private static async Task<QuizAnswer> ReadFirstQuizAnswerAsync(HttpResponseMessage response)
    {
        using var quiz = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var question = quiz.RootElement.GetProperty("data").GetProperty("questions")[0];
        return new QuizAnswer(
            question.GetProperty("id").GetInt32(),
            question.GetProperty("choices")[0].GetProperty("id").GetInt32());
    }

    private async Task<SeededLearningJourney> SeedLearningJourneyAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var user = await userManager.FindByEmailAsync("user@test.com")
            ?? throw new InvalidOperationException("The integration-test user was not seeded.");

        var suffix = Guid.NewGuid().ToString("N");
        var course = new Course
        {
            Name = $"رحلة تعلم {suffix}",
            Description = "دورة اختبارية لرحلة التعلم الكاملة",
            Author = "Integration Tests",
            CourseLanguage = "العربية",
            CourseVideosCount = 2,
            ImageUrl = "https://example.test/learning-journey.jpg",
            rating = 5,
            Category = new Category
            {
                Name = $"فئة {suffix}",
                Description = "فئة اختبارية",
                ImageUrl = "https://example.test/category.jpg",
                Courses = []
            },
            Videos =
            [
                new Video
                {
                    Title = "الدرس الأول",
                    Description = "الدرس الأول",
                    VideoUrl = "https://youtu.be/integration001",
                    ThumbnailUrl = "https://example.test/video-1.jpg",
                    VideoDuration = TimeSpan.FromSeconds(100),
                    OrderInCourse = 1
                },
                new Video
                {
                    Title = "الدرس الثاني",
                    Description = "الدرس الثاني",
                    VideoUrl = "https://youtu.be/integration002",
                    ThumbnailUrl = "https://example.test/video-2.jpg",
                    VideoDuration = TimeSpan.FromSeconds(100),
                    OrderInCourse = 2
                }
            ],
            Quizzes =
            [
                new Quiz
                {
                    Name = "اختبار رحلة التعلم",
                    Description = "اختبار تكامل",
                    Questions =
                    [
                        new Question
                        {
                            Text = "ما الإجابة الصحيحة؟",
                            Choices =
                            [
                                new Choice { Text = "الإجابة الصحيحة" },
                                new Choice { Text = "إجابة أخرى" }
                            ]
                        }
                    ],
                    Progresses = [],
                    Certificates = []
                }
            ],
            enrollments = [],
            Review = []
        };

        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync();

        var question = course.Quizzes.Single().Questions.Single();
        question.CorrectChoiceId = question.Choices[0].Id;
        await dbContext.SaveChangesAsync();

        return new SeededLearningJourney(
            course.Id,
            course.Videos.Single(video => video.OrderInCourse == 1).Id,
            course.Videos.Single(video => video.OrderInCourse == 2).Id,
            course.Quizzes.Single().Id);
    }

    private sealed record SeededLearningJourney(
        int CourseId,
        int FirstVideoId,
        int SecondVideoId,
        int QuizId);

    private sealed record QuizAnswer(int QuestionId, int ChoiceId);
}
