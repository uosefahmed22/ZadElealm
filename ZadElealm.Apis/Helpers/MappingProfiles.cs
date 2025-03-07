using AutoMapper;
using ZadElealm.Apis.Controllers.Deketeed;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ZadElealm.Apis.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Category, CategoryResponseDto>();
            CreateMap<Category, CategoryWithCoursesDto>();

            CreateMap<Course, CourseResponseWithAllDataDto>();
            CreateMap<Video, VideoDto>();
            CreateMap<Quiz, QuizDto>();


        }
    }
}
