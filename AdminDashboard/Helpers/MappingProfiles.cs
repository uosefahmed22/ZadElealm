using AdminDashboard.Commands;
using AutoMapper;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models;

namespace AdminDashboard.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {

            CreateMap<Category, CategoryResponseDto>().ReverseMap();
            CreateMap<Category, CategoryWithCoursesDto>();

            CreateMap<Category, CreateCategoryDto>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.ImageUrl != null ? src.ImageUrl.FileName : null));
            CreateMap<CreateCategoryDto, UpdateCategoryCommand>();

            CreateMap<Course, CourseDto>();
            CreateMap<Course, CourseResponseWithAllDataDto>();

            CreateMap<CreateCategoryDto, CreateCategoryCommand>();

            CreateMap<CreateCategoryCommand, Category>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()); 

            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.ImageUrl != null ? src.ImageUrl.FileName : null));
        }
    }
}
