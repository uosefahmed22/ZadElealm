using AutoMapper;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Apis.Dtos.DtosCategory;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.ServiceDto;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ZadElealm.Apis.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Category, CategoryResponseDto>().ReverseMap();
            CreateMap<Category, CategoryWithCoursesDto>();
            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl.FileName))
                .ReverseMap();

            CreateMap<Certificate, CertificateDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.DisplayName))
                .ForMember(dest => dest.QuizName, opt => opt.MapFrom(src => src.Quiz.Name));

            CreateMap<Course, CourseDto>();
            CreateMap<Course, CourseResponseWithAllDataDto>();

            CreateMap<Video, VideoDto>();
            CreateMap<Quiz, QuizResponseForCourseDto>();
            CreateMap<Video, VideoWithUserProgressDto>()
                .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.WatchedDuration, opt => opt.Ignore());

            CreateMap<Quiz, QuizResponseDto>();
            CreateMap<Question, QuestionDto>();
            CreateMap<Choice, ChoiceDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Text));

            CreateMap<Quiz, QuizDto>();
            CreateMap<Choice, ChoiceDto>();
            CreateMap<Progress, ProgressDto>();

            CreateMap<ReportDto, Report>();

            CreateMap<ReviewDto, Review>();

            CreateMap<Reply, ReplyDto>()
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.User.DisplayName))
                .ForMember(dest => dest.UserImage, opt => opt.MapFrom(src => src.User.ImageUrl))
                .ForMember(dest => dest.AppUserId, opt => opt.MapFrom(src => src.User.Id))
                .ForMember(dest => dest.ReplyLikesCount, opt => opt.MapFrom(src => src.ReplyLikes.Count));

            CreateMap<Review, ReviewWithReviwerDataDto>()
                .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.User.DisplayName))
                .ForMember(d => d.AppUserId, o => o.MapFrom(s => s.User.Id))
                .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.User.ImageUrl))
                .ForMember(dest => dest.HasReplies, opt => opt.MapFrom(src => src.Replies.Any()))
                .ForMember(dest => dest.RepliesCount, opt => opt.MapFrom(src => src.Replies.Count))
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.LikesCount));

            CreateMap<Reply, ReplyDto>()
                .ForMember(dest => dest.DisplayName, opt =>
                    opt.MapFrom(src => src.User.DisplayName))
                .ForMember(dest => dest.UserImage, opt =>
                    opt.MapFrom(src => src.User.ImageUrl));

            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.IsRead, opt =>
                    opt.MapFrom(src => src.UserNotifications.FirstOrDefault().IsRead));

            CreateMap<Notification, NotificationsResponse>();

            CreateMap<AppUser, UserProfileDTO>();

            CreateMap<UserRank, UserRankDto>()
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.DisplayName))
                .ForMember(d => d.UserImage, o => o.MapFrom(s => s.User.ImageUrl))
                .ForMember(d => d.Rank, o => o.MapFrom(s => s.Rank));
        }
    }
}