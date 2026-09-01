using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.IntegrationTests;

public class CourseFeedbackTests : IClassFixture<ZadElealmApiFactory>
{
    private const string SecondUserEmail = "feedback-owner-check@test.com";
    private readonly ZadElealmApiFactory _factory;

    public CourseFeedbackTests(ZadElealmApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task EnrolledStudent_CanRateAndReviewOnce_AndOnlyOwnerCanDeleteReview()
    {
        var courseId = await GetSeededCourseIdAsync();
        using var student = CreateAuthenticatedClient("user@test.com");

        var reviewBeforeEnrollment = await AddReviewAsync(student, courseId, "مراجعة واضحة قبل التسجيل");
        Assert.Equal(HttpStatusCode.BadRequest, reviewBeforeEnrollment.StatusCode);

        var ratingBeforeEnrollment = await AddRatingAsync(student, courseId, 5);
        Assert.Equal(HttpStatusCode.BadRequest, ratingBeforeEnrollment.StatusCode);

        var enrollment = await student.PostAsync($"/api/Enrollment/{courseId}", null);
        Assert.Equal(HttpStatusCode.OK, enrollment.StatusCode);

        var invalidRating = await AddRatingAsync(student, courseId, 6);
        Assert.Equal(HttpStatusCode.BadRequest, invalidRating.StatusCode);

        var rating = await AddRatingAsync(student, courseId, 5);
        Assert.Equal(HttpStatusCode.OK, rating.StatusCode);

        var duplicateRating = await AddRatingAsync(student, courseId, 4);
        Assert.Equal(HttpStatusCode.BadRequest, duplicateRating.StatusCode);

        var canRate = await student.GetAsync($"/api/Rating/can-rate/{courseId}");
        Assert.Equal(HttpStatusCode.OK, canRate.StatusCode);
        using (var canRateDocument = JsonDocument.Parse(await canRate.Content.ReadAsStringAsync()))
        {
            Assert.False(canRateDocument.RootElement.GetProperty("data").GetBoolean());
        }

        var shortReview = await AddReviewAsync(student, courseId, "قصيرة");
        Assert.Equal(HttpStatusCode.BadRequest, shortReview.StatusCode);

        var review = await AddReviewAsync(student, courseId, "دورة نافعة وشرحها واضح ومنظم");
        Assert.Equal(HttpStatusCode.OK, review.StatusCode);

        var duplicateReview = await AddReviewAsync(student, courseId, "مراجعة ثانية لنفس المستخدم والدورة");
        Assert.Equal(HttpStatusCode.BadRequest, duplicateReview.StatusCode);

        var feedback = await ReadCourseFeedbackAsync(student, courseId);
        Assert.Equal(5m, feedback.Rating);
        var ownedReview = Assert.Single(feedback.Reviews);
        Assert.True(ownedReview.IsOwnedByCurrentUser);
        Assert.Equal("دورة نافعة وشرحها واضح ومنظم", ownedReview.Text);

        var likeReview = await student.PostAsync($"/api/Review/{ownedReview.Id}/like", null);
        Assert.Equal(HttpStatusCode.OK, likeReview.StatusCode);

        var shortReply = await student.PostAsJsonAsync(
            $"/api/Reply/review/{ownedReview.Id}",
            new { replyText = "ا" });
        Assert.Equal(HttpStatusCode.BadRequest, shortReply.StatusCode);

        var addReply = await student.PostAsJsonAsync(
            $"/api/Reply/review/{ownedReview.Id}",
            new { replyText = "رد مفيد على المراجعة" });
        Assert.Equal(HttpStatusCode.OK, addReply.StatusCode);

        var replies = await ReadRepliesAsync(student, ownedReview.Id);
        var ownedReply = Assert.Single(replies);
        Assert.True(ownedReply.IsOwnedByCurrentUser);
        Assert.False(ownedReply.IsLikedByCurrentUser);

        var likeReply = await student.PostAsync($"/api/Reply/{ownedReply.Id}/like", null);
        Assert.Equal(HttpStatusCode.OK, likeReply.StatusCode);

        replies = await ReadRepliesAsync(student, ownedReview.Id);
        ownedReply = Assert.Single(replies);
        Assert.True(ownedReply.IsLikedByCurrentUser);
        Assert.Equal(1, ownedReply.LikesCount);

        feedback = await ReadCourseFeedbackAsync(student, courseId);
        ownedReview = Assert.Single(feedback.Reviews);
        Assert.True(ownedReview.IsLikedByCurrentUser);
        Assert.Equal(1, ownedReview.LikesCount);

        await EnsureSecondUserAsync();
        using var otherStudent = CreateAuthenticatedClient(SecondUserEmail);
        var secondEnrollment = await otherStudent.PostAsync($"/api/Enrollment/{courseId}", null);
        Assert.Equal(HttpStatusCode.OK, secondEnrollment.StatusCode);

        var forbiddenDelete = await otherStudent.DeleteAsync($"/api/Review/{ownedReview.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenDelete.StatusCode);

        var forbiddenReplyDelete = await otherStudent.DeleteAsync($"/api/Reply/{ownedReply.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenReplyDelete.StatusCode);

        var deleteReply = await student.DeleteAsync($"/api/Reply/{ownedReply.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteReply.StatusCode);
        Assert.Empty(await ReadRepliesAsync(student, ownedReview.Id));

        var delete = await student.DeleteAsync($"/api/Review/{ownedReview.Id}");
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);

        var afterDelete = await ReadCourseFeedbackAsync(student, courseId);
        Assert.Empty(afterDelete.Reviews);

        var addAfterSoftDelete = await AddReviewAsync(student, courseId, "مراجعة جديدة بعد حذف المراجعة السابقة");
        Assert.Equal(HttpStatusCode.OK, addAfterSoftDelete.StatusCode);
    }

    [Fact]
    public async Task FeedbackEndpoints_RequireAuthentication()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        var rating = await client.PostAsJsonAsync("/api/Rating", new { value = 5, courseId = 1 });
        var review = await client.PostAsJsonAsync(
            "/api/Review",
            new { reviewText = "مراجعة بدون تسجيل دخول", courseId = 1 });
        var canRate = await client.GetAsync("/api/Rating/can-rate/1");
        var reviewLike = await client.PostAsync("/api/Review/1/like", null);
        var replies = await client.GetAsync("/api/Reply/review/1");
        var addReply = await client.PostAsJsonAsync(
            "/api/Reply/review/1",
            new { replyText = "رد بدون تسجيل دخول" });
        var replyLike = await client.PostAsync("/api/Reply/1/like", null);
        var deleteReply = await client.DeleteAsync("/api/Reply/1");

        Assert.Equal(HttpStatusCode.Unauthorized, rating.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, review.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, canRate.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, reviewLike.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, replies.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, addReply.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, replyLike.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, deleteReply.StatusCode);
    }

    private HttpClient CreateAuthenticatedClient(string email)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            _factory.GenerateTokenForEmail(email, "User"));
        return client;
    }

    private static Task<HttpResponseMessage> AddReviewAsync(
        HttpClient client,
        int courseId,
        string reviewText)
    {
        return client.PostAsJsonAsync("/api/Review", new { reviewText, courseId });
    }

    private static Task<HttpResponseMessage> AddRatingAsync(
        HttpClient client,
        int courseId,
        int value)
    {
        return client.PostAsJsonAsync("/api/Rating", new { value, courseId });
    }

    private async Task<int> GetSeededCourseIdAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await dbContext.Courses
            .OrderBy(course => course.Id)
            .Select(course => course.Id)
            .FirstAsync();
    }

    private async Task EnsureSecondUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        if (await userManager.FindByEmailAsync(SecondUserEmail) != null) return;

        var user = new AppUser
        {
            DisplayName = "Feedback Ownership Test",
            Email = SecondUserEmail,
            UserName = SecondUserEmail,
            EmailConfirmed = true
        };
        var created = await userManager.CreateAsync(user, ZadElealmApiFactory.TestUserPassword);
        Assert.True(created.Succeeded);
        var role = await userManager.AddToRoleAsync(user, "User");
        Assert.True(role.Succeeded);
    }

    private static async Task<CourseFeedback> ReadCourseFeedbackAsync(HttpClient client, int courseId)
    {
        var response = await client.GetAsync($"/api/Course/{courseId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        var reviews = data.GetProperty("review")
            .EnumerateArray()
            .Select(review => new FeedbackReview(
                review.GetProperty("id").GetInt32(),
                review.GetProperty("text").GetString() ?? string.Empty,
                review.GetProperty("isOwnedByCurrentUser").GetBoolean(),
                review.GetProperty("isLikedByCurrentUser").GetBoolean(),
                review.GetProperty("likesCount").GetInt32()))
            .ToList();
        return new CourseFeedback(data.GetProperty("rating").GetDecimal(), reviews);
    }

    private static async Task<IReadOnlyList<FeedbackReply>> ReadRepliesAsync(
        HttpClient client,
        int reviewId)
    {
        var response = await client.GetAsync($"/api/Reply/review/{reviewId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data")
            .EnumerateArray()
            .Select(reply => new FeedbackReply(
                reply.GetProperty("id").GetInt32(),
                reply.GetProperty("isOwnedByCurrentUser").GetBoolean(),
                reply.GetProperty("isLikedByCurrentUser").GetBoolean(),
                reply.GetProperty("replyLikesCount").GetInt32()))
            .ToList();
    }

    private sealed record FeedbackReview(
        int Id,
        string Text,
        bool IsOwnedByCurrentUser,
        bool IsLikedByCurrentUser,
        int LikesCount);
    private sealed record FeedbackReply(
        int Id,
        bool IsOwnedByCurrentUser,
        bool IsLikedByCurrentUser,
        int LikesCount);
    private sealed record CourseFeedback(decimal Rating, IReadOnlyList<FeedbackReview> Reviews);
}
