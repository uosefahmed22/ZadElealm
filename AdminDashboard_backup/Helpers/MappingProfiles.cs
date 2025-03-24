using AdminDashboard.Commands.CategoryCommand;
using AdminDashboard.Commands.CourseCommand;
using AdminDashboard.Models;
using AutoMapper;
using Org.BouncyCastle.Asn1.Cmp;
using ZadElealm.Apis.Dtos.DtosCategory;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Core.Enums;
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

            CreateMap<CreateCourseCommand, CourseViewModel>().ReverseMap();

            CreateMap<Course, CourseDto>();
            CreateMap<Course, CourseResponseWithAllDataDto>();

            CreateMap<CreateCategoryDto, CreateCategoryCommand>();

            CreateMap<Report,AdminDashboard.Dto.ReportDto>()
                .ForMember(dest => dest.reportTypes, opt => opt.MapFrom(src => src.reportTypes.ToString()));

            CreateMap<Dto.ReportDto, Report>()
                .ForMember(dest => dest.reportTypes, opt => opt.MapFrom(src => Enum.Parse<ReportType>(src.reportTypes)));
            CreateMap<AdminDashboard.Dto.HandleReportDto, Report>().ReverseMap();

            CreateMap<CreateCategoryCommand, Category>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()); 

            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.ImageUrl != null ? src.ImageUrl.FileName : null));
        }
    }
}
