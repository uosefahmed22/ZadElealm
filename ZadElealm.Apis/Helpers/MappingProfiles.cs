using AutoMapper;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.ServiceDto;
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

            CreateMap<Quiz, QuizResponseDto>();
            CreateMap<Question, QuestionDto>();
            CreateMap<Choice, ChoiceDto>();
            CreateMap<Progress, ProgressDto>();

            CreateMap<ReportDto, Report>();

            CreateMap<ReviewDto, Review>();

            CreateMap<Notification, NotificationDto>();
            CreateMap<Notification, NotificationsResponse>();

        }
    }
}
