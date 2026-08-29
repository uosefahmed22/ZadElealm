using System;
using System.Collections.Generic;
using System.Linq;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Dtos.DtosCategory;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Core.Models;

namespace ZadElealm.Apis.Mappers
{
    public static class CourseMappingExtensions
    {
        public static CourseDto ToDto(this Course entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new CourseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Author = entity.Author,
                CourseLanguage = entity.CourseLanguage,
                CourseVideosCount = entity.CourseVideosCount,
                rating = entity.rating,
                ImageUrl = entity.ImageUrl,
                Category = entity.Category?.ToDto(),
                CreatedAt = entity.CreatedAt
            };
        }

        public static IReadOnlyList<CourseDto> ToDtos(this IEnumerable<Course> entities)
        {
            if (entities is null) return [];
            return entities.Select(e => e.ToDto()).ToList();
        }

        public static CourseResponseWithAllDataDto ToDetailsDto(this Course entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new CourseResponseWithAllDataDto
            {
                Name = entity.Name,
                Description = entity.Description,
                Author = entity.Author,
                rating = entity.rating,
                CourseLanguage = entity.CourseLanguage,
                CourseVideosCount = entity.CourseVideosCount,
                ImageUrl = entity.ImageUrl,
                CreatedAt = entity.CreatedAt,
                TotalEnrolledStudents = entity.TotalEnrolledStudents,
                Category = entity.Category?.ToDto(),
                Videos = entity.Videos != null
                    ? entity.Videos.Select(v => v.ToVideoWithUserProgressDto()).ToList()
                    : new List<VideoWithUserProgressDto>(),
                Review = entity.Review != null
                    ? entity.Review.Select(r => r.ToReviewWithReviwerDataDto()).ToList()
                    : new List<ReviewWithReviwerDataDto>(),
                Quizzes = entity.Quizzes != null
                    ? entity.Quizzes.Select(q => q.ToCourseQuizDto()).ToList()
                    : new List<QuizResponseForCourseDto>()
            };
        }

        public static VideoDto ToDto(this Video video)
        {
            ArgumentNullException.ThrowIfNull(video);

            return new VideoDto
            {
                Id = video.Id,
                Title = video.Title,
                Description = video.Description,
                VideoUrl = video.VideoUrl,
                ThumbnailUrl = video.ThumbnailUrl,
                VideoDuration = video.VideoDuration
            };
        }

        public static IReadOnlyList<VideoDto> ToDtos(this IEnumerable<Video> videos)
        {
            if (videos is null) return [];
            return videos.Select(v => v.ToDto()).ToList();
        }

        public static VideoWithUserProgressDto ToVideoWithUserProgressDto(this Video video)
        {
            ArgumentNullException.ThrowIfNull(video);

            return new VideoWithUserProgressDto
            {
                Id = video.Id,
                Title = video.Title,
                Description = video.Description,
                VideoUrl = video.VideoUrl,
                ThumbnailUrl = video.ThumbnailUrl,
                VideoDuration = video.VideoDuration,
                IsCompleted = false,
                WatchedDuration = TimeSpan.Zero
            };
        }

        public static IReadOnlyList<VideoWithUserProgressDto> ToVideoWithUserProgressDtos(this IEnumerable<Video> videos)
        {
            if (videos is null) return [];
            return videos.Select(v => v.ToVideoWithUserProgressDto()).ToList();
        }

        public static QuizResponseForCourseDto ToCourseQuizDto(this Quiz quiz)
        {
            ArgumentNullException.ThrowIfNull(quiz);

            return new QuizResponseForCourseDto
            {
                Id = quiz.Id,
                Name = quiz.Name,
                Description = quiz.Description,
                CreatedAt = quiz.CreatedAt
            };
        }

        public static IReadOnlyList<QuizResponseForCourseDto> ToCourseQuizDtos(this IEnumerable<Quiz> quizzes)
        {
            if (quizzes is null) return [];
            return quizzes.Select(q => q.ToCourseQuizDto()).ToList();
        }

        public static ReviewWithReviwerDataDto ToReviewWithReviwerDataDto(this Review entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new ReviewWithReviwerDataDto
            {
                Id = entity.Id,
                Text = entity.Text,
                CourseId = entity.CourseId,
                CreatedAt = entity.CreatedAt,
                AppUserId = entity.User?.Id ?? entity.AppUserId,
                DisplayName = entity.User?.DisplayName,
                ImageUrl = entity.User?.ImageUrl,
                HasReplies = entity.Replies != null && entity.Replies.Count > 0,
                RepliesCount = entity.Replies != null ? entity.Replies.Count : 0,
                LikesCount = entity.Likes != null ? entity.Likes.Count : entity.LikesCount
            };
        }

        public static IReadOnlyList<ReviewWithReviwerDataDto> ToReviewWithReviwerDataDtos(this IEnumerable<Review> reviews)
        {
            if (reviews is null) return [];
            return reviews.Select(r => r.ToReviewWithReviwerDataDto()).ToList();
        }
    }
}
