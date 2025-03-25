using AdminDashboard.Commands.CategoryCommand;
using AdminDashboard.Dto;
using AutoMapper;
using Org.BouncyCastle.Asn1.Cmp;
using ZadElealm.Apis.Dtos;
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

            CreateMap<Category, DashboardCategoryDto>().ReverseMap();
            CreateMap<Course, DashboardCourseDto>()
                 .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.ImageUrl))
                 .ReverseMap()
                 .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Image));





            CreateMap<Category, CategoryResponseDto>().ReverseMap();
            CreateMap<Category, CategoryWithCoursesDto>();


            CreateMap<Category, Dto.CreateCategoryDto>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore());

            CreateMap<Dto.CreateCategoryDto, Category>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.ImageUrl != null ? src.ImageUrl.FileName : null));
            CreateMap<Dto.CreateCategoryDto, UpdateCategoryCommand>();

            CreateMap<Course, CourseResponseWithAllDataDto>();

            CreateMap<Dto.CreateCategoryDto, CreateCategoryCommand>();

            CreateMap<Report, ReportDto>()
    .ForMember(dest => dest.reportTypes, opt => opt.MapFrom(src => src.reportTypes.ToString()));

            CreateMap<ReportDto, Report>()
                .ForMember(dest => dest.reportTypes, opt => opt.MapFrom(src => Enum.Parse<ReportType>(src.reportTypes)));
            CreateMap<AdminDashboard.Dto.HandleReportDto, Report>().ReverseMap();

            CreateMap<CreateCategoryCommand, Category>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()); 

            CreateMap<Dto.CreateCategoryDto, Category>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.ImageUrl != null ? src.ImageUrl.FileName : null));
        }
    }
}
