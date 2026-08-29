using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Apis.Mappers;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Service.Mappers;

namespace ZadElealm.UnitTests.Mappers;

public class MappingExtensionsTests
{
    // ─── 1. Course details mapping ───────────────────────────────────────────

    [Fact]
    public void CourseToDetailsDto_MapsAllFields()
    {
        var category = new Category { Id = 2, Name = "Fiqh", Description = "desc", ImageUrl = "cat.png" };
        var video = new Video
        {
            Id = 10, Title = "Intro", Description = "v-desc",
            VideoUrl = "v.mp4", ThumbnailUrl = "thumb.jpg",
            VideoDuration = TimeSpan.FromMinutes(5)
        };
        var quiz = new Quiz { Id = 3, Name = "Q1", Description = "quiz-desc" };
        var reviewer = new AppUser { Id = "u1", DisplayName = "Ahmed", ImageUrl = "img.jpg" };
        var review = new Review
        {
            Id = 20, Text = "Great!", CourseId = 1,
            AppUserId = "u1", User = reviewer,
            Replies = new List<Reply>(),
            Likes = new List<ReviewLike>()
        };
        var course = new Course
        {
            Id = 1, Name = "Tawheed", Description = "core", Author = "Sheikh",
            CourseLanguage = "Arabic", CourseVideosCount = 1, rating = 4.5M,
            ImageUrl = "c.png", CreatedAt = new DateTime(2025, 1, 1),
            enrollments = new List<Enrollment>
            {
                new Enrollment { Id = 1, CourseId = 1, AppUserId = "u1" },
                new Enrollment { Id = 2, CourseId = 1, AppUserId = "u2" }
            },
            Category = category,
            Videos = new List<Video> { video },
            Quizzes = new List<Quiz> { quiz },
            Review = new List<Review> { review }
        };

        var dto = course.ToDetailsDto();

        Assert.Equal("Tawheed", dto.Name);
        Assert.Equal("Sheikh", dto.Author);
        Assert.Equal(2, dto.TotalEnrolledStudents);
        Assert.Equal("Fiqh", dto.Category?.Name);
        Assert.Single(dto.Videos);
        Assert.False(dto.Videos.First().IsCompleted);
        Assert.Equal(TimeSpan.Zero, dto.Videos.First().WatchedDuration);
        Assert.Single(dto.Quizzes);
        Assert.Single(dto.Review);
        Assert.Equal("Ahmed", dto.Review.First().DisplayName);
    }

    // ─── 2. Quiz mapping ─────────────────────────────────────────────────────

    [Fact]
    public void QuizToResponseDto_MapsQuestionsAndChoices()
    {
        var choice = new Choice { Id = 1, Text = "Choice A" };
        var question = new Question
        {
            Id = 5, Text = "What is Iman?", QuizId = 3,
            Choices = new List<Choice> { choice }
        };
        var quiz = new Quiz
        {
            Id = 3, Name = "Aqeedah Quiz", Description = "test",
            Questions = new List<Question> { question }
        };

        var dto = quiz.ToResponseDto();

        Assert.Equal(3, dto.Id);
        Assert.Equal("Aqeedah Quiz", dto.Name);
        Assert.Single(dto.Questions);
        Assert.Equal("What is Iman?", dto.Questions.First().Text);
        Assert.Single(dto.Questions.First().Choices);
        Assert.Equal("Choice A", dto.Questions.First().Choices.First().Text);
    }

    // ─── 3. Notification mapping (IsRead from UserNotification) ──────────────

    [Fact]
    public void UserNotificationToDto_TakesIsReadFromJoinEntity()
    {
        var notification = new Notification
        {
            Id = 7, Title = "New lesson",
            Description = "Added lesson", Type = NotificationType.System
        };

        // Test IsRead = false
        var unread = new UserNotification
        {
            NotificationId = 7, Notification = notification,
            AppUserId = "u1", IsRead = false
        };
        var unreadDto = unread.ToDto();
        Assert.Equal(7, unreadDto.Id);
        Assert.Equal("New lesson", unreadDto.Title);
        Assert.False(unreadDto.IsRead);

        // Test IsRead = true
        var read = new UserNotification
        {
            NotificationId = 7, Notification = notification,
            AppUserId = "u1", IsRead = true
        };
        var readDto = read.ToDto();
        Assert.True(readDto.IsRead);
    }

    // ─── 4. UserRank mapping ─────────────────────────────────────────────────

    [Fact]
    public void UserRankToDto_MapsDisplayNameImageAndStats()
    {
        var user = new AppUser { Id = "u5", DisplayName = "Khalid", ImageUrl = "k.jpg" };
        var rank = new UserRank
        {
            Id = 1, UserId = "u5", User = user,
            TotalPoints = 350, Rank = UserRankEnum.Silver,
            CompletedCoursesCount = 5, CertificatesCount = 2,
            AverageQuizScore = 88.5, LastUpdated = DateTime.UtcNow
        };

        var dto = rank.ToDto();

        Assert.Equal("u5", dto.UserId);
        Assert.Equal("Khalid", dto.UserName);
        Assert.Equal("k.jpg", dto.UserImage);
        Assert.Equal(350, dto.TotalPoints);
        Assert.Equal(UserRankEnum.Silver, dto.Rank);
        Assert.Equal(5, dto.CompletedCoursesCount);
        Assert.Equal(2, dto.CertificatesCount);
        Assert.Equal(88.5, dto.AverageQuizScore);
    }

    // ─── 5. Report mapping (enum/string conversions) ─────────────────────────

    [Fact]
    public void ReportDtoToEntity_ParsesEnumFromString()
    {
        var dto = new ReportDto
        {
            TitleOfTheIssue = "Bug",
            Description = "Something is broken",
            reportTypes = "Technical",
            IsSolved = false
        };

        var entity = dto.ToEntity();

        Assert.Equal("Bug", entity.TitleOfTheIssue);
        Assert.Equal(ReportType.Technical, entity.reportTypes);
        Assert.False(entity.IsSolved);
    }

    [Fact]
    public void ReportToDto_SerializesEnumToString()
    {
        var report = new Report
        {
            Id = 11,
            TitleOfTheIssue = "Content error",
            Description = "Wrong video",
            reportTypes = ReportType.ProductIssue,
            AdminResponse = "Fixed",
            IsSolved = true
        };

        var dto = report.ToDto();

        Assert.Equal(11, dto.Id);
        Assert.Equal("ProductIssue", dto.reportTypes);
        Assert.True(dto.IsSolved);
        Assert.Equal("Fixed", dto.AdminResponse);
    }
}
