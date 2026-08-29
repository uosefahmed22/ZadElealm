using System;
using System.Collections.Generic;
using System.Linq;
using ZadElealm.Apis.Dtos.DtosCategory;
using ZadElealm.Apis.Dtos.DtosCourse;
using ZadElealm.Core.Models;

namespace ZadElealm.Apis.Mappers
{
    public static class CategoryMappingExtensions
    {
        public static CategoryResponseDto ToDto(this Category entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new CategoryResponseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                ImageUrl = entity.ImageUrl
            };
        }

        public static IReadOnlyList<CategoryResponseDto> ToDtos(this IEnumerable<Category> entities)
        {
            if (entities is null) return [];
            return entities.Select(e => e.ToDto()).ToList();
        }

        public static CategoryWithCoursesDto ToCategoryWithCoursesDto(this Category entity, IEnumerable<Course>? courses = null)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var courseList = courses ?? entity.Courses;

            return new CategoryWithCoursesDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Courses = courseList != null ? courseList.Select(c => c.ToDto()).ToList() : new List<CourseDto>()
            };
        }
    }
}
