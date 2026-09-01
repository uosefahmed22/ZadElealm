using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ZadElealm.Core.Models;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.IntegrationTests;

public sealed class PersistenceUniquenessTests : IClassFixture<ZadElealmApiFactory>
{
    private readonly ZadElealmApiFactory _factory;

    public PersistenceUniquenessTests(ZadElealmApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void LearningProgressUniquenessMigration_IsDiscoverable()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Contains(
            "20260831120000_AddLearningProgressUniqueness",
            dbContext.Database.GetMigrations());
    }

    [Fact]
    public void CourseFeedbackUniquenessMigration_IsDiscoverable()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Contains(
            "20260901020000_AddCourseFeedbackUniqueness",
            dbContext.Database.GetMigrations());
    }

    [Fact]
    public void FeedbackInteractionUniquenessMigration_IsDiscoverable()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Contains(
            "20260901030000_AddFeedbackInteractionUniqueness",
            dbContext.Database.GetMigrations());
    }

    [Fact]
    public async Task ReviewLike_RejectsSecondRowForSameUserAndReview()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await dbContext.Users.Select(user => user.Id).FirstAsync();
        var courseId = (await CreateCourseAsync(dbContext, "Review-like uniqueness course")).Id;
        var review = await GetOrCreateReviewAsync(dbContext, userId, courseId);

        dbContext.ReviewLikes.Add(new ReviewLike { AppUserId = userId, ReviewId = review.Id });
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        dbContext.ReviewLikes.Add(new ReviewLike { AppUserId = userId, ReviewId = review.Id });
        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task ReplyLike_RejectsSecondRowForSameUserAndReply()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await dbContext.Users.Select(user => user.Id).FirstAsync();
        var courseId = (await CreateCourseAsync(dbContext, "Reply-like uniqueness course")).Id;
        var review = await GetOrCreateReviewAsync(dbContext, userId, courseId);
        var reply = new Reply
        {
            AppUserId = userId,
            ReviewId = review.Id,
            Text = "Reply for like uniqueness"
        };
        dbContext.Add(reply);
        await dbContext.SaveChangesAsync();

        dbContext.Add(new ReplyLike { AppUserId = userId, ReplyId = reply.Id });
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        dbContext.Add(new ReplyLike { AppUserId = userId, ReplyId = reply.Id });
        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Review_RejectsSecondActiveRowForSameUserAndCourse()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await dbContext.Users.Select(user => user.Id).FirstAsync();
        var courseId = await dbContext.Courses.Select(course => course.Id).FirstAsync();

        dbContext.Reviews.Add(new Review
        {
            AppUserId = userId,
            CourseId = courseId,
            Text = "First persistence review"
        });
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        dbContext.Reviews.Add(new Review
        {
            AppUserId = userId,
            CourseId = courseId,
            Text = "Second persistence review"
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Rating_RejectsSecondActiveRowForSameUserAndCourse()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await dbContext.Users.Select(user => user.Id).FirstAsync();
        var courseId = await dbContext.Courses.Select(course => course.Id).FirstAsync();

        dbContext.Ratings.Add(new Rating
        {
            AppUserId = userId,
            courseId = courseId,
            Value = 4
        });
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        dbContext.Ratings.Add(new Rating
        {
            AppUserId = userId,
            courseId = courseId,
            Value = 5
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task VideoProgress_RejectsSecondActiveRowForSameUserAndVideo()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await dbContext.Users.Select(user => user.Id).FirstAsync();
        var courseId = await dbContext.Courses.Select(course => course.Id).FirstAsync();
        var video = new Video
        {
            CourseId = courseId,
            Title = "Concurrency video",
            Description = "Integration-test video",
            VideoUrl = "https://example.test/video",
            ThumbnailUrl = "https://example.test/video.jpg",
            VideoDuration = TimeSpan.FromMinutes(5),
            OrderInCourse = 50
        };
        dbContext.Videos.Add(video);
        await dbContext.SaveChangesAsync();

        dbContext.VideoProgresses.Add(new VideoProgress
        {
            UserId = userId,
            CourseId = courseId,
            VideoId = video.Id,
            WatchedDuration = TimeSpan.FromMinutes(1)
        });
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        dbContext.VideoProgresses.Add(new VideoProgress
        {
            UserId = userId,
            CourseId = courseId,
            VideoId = video.Id,
            WatchedDuration = TimeSpan.FromMinutes(2)
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task QuizProgress_RejectsSecondActiveRowForSameUserAndQuiz()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await dbContext.Users.Select(user => user.Id).FirstAsync();
        var courseId = await dbContext.Courses.Select(course => course.Id).FirstAsync();
        var quiz = await CreateQuizAsync(dbContext, courseId, "Concurrency progress quiz");

        dbContext.Progresses.Add(new Progress
        {
            AppUserId = userId,
            QuizId = quiz.Id,
            Score = 40
        });
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        dbContext.Progresses.Add(new Progress
        {
            AppUserId = userId,
            QuizId = quiz.Id,
            Score = 80,
            IsCompleted = true
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Certificate_RejectsSecondActiveRowForSameUserAndQuiz()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await dbContext.Users.Select(user => user.Id).FirstAsync();
        var courseId = await dbContext.Courses.Select(course => course.Id).FirstAsync();
        var quiz = await CreateQuizAsync(dbContext, courseId, "Concurrency certificate quiz");

        dbContext.Certificates.Add(CreateCertificate(userId, quiz.Id, "first"));
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        dbContext.Certificates.Add(CreateCertificate(userId, quiz.Id, "second"));

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    private static async Task<Quiz> CreateQuizAsync(AppDbContext dbContext, int courseId, string name)
    {
        var quiz = new Quiz
        {
            CourseId = courseId,
            Name = name,
            Description = "Integration-test quiz",
            Questions = []
        };
        dbContext.Quizzes.Add(quiz);
        await dbContext.SaveChangesAsync();
        return quiz;
    }

    private static async Task<Course> CreateCourseAsync(
        AppDbContext dbContext,
        string name)
    {
        var categoryId = await dbContext.Categories.Select(category => category.Id).FirstAsync();
        var course = new Course
        {
            Name = $"{name} {Guid.NewGuid():N}",
            Description = "Integration-test course",
            Author = "Integration tests",
            CourseLanguage = "Arabic",
            ImageUrl = "https://example.test/course.jpg",
            CategoryId = categoryId
        };
        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync();
        return course;
    }

    private static async Task<Review> GetOrCreateReviewAsync(
        AppDbContext dbContext,
        string userId,
        int courseId)
    {
        var existing = await dbContext.Reviews
            .FirstOrDefaultAsync(review => review.AppUserId == userId && review.CourseId == courseId);
        if (existing != null) return existing;

        var review = new Review
        {
            AppUserId = userId,
            CourseId = courseId,
            Text = "Review for feedback interaction uniqueness"
        };
        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();
        return review;
    }

    private static Certificate CreateCertificate(string userId, int quizId, string suffix)
        => new()
        {
            UserId = userId,
            QuizId = quizId,
            Name = $"Certificate {suffix}",
            Description = "Integration-test certificate",
            PdfUrl = $"https://example.test/{suffix}.pdf"
        };
}
