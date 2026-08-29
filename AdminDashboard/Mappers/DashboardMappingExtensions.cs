using System;
using System.Collections.Generic;
using System.Linq;
using AdminDashboard.Commands.CategoryCommand;
using AdminDashboard.Dto;
using ZadElealm.Apis.Dtos.DtosCategory;
using ZadElealm.Core.Models;

namespace AdminDashboard.Mappers
{
    public static class DashboardMappingExtensions
    {
        public static DashboardCategoryDto ToDashboardDto(this Category entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new DashboardCategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                ImageUrl = entity.ImageUrl
            };
        }

        public static IReadOnlyList<DashboardCategoryDto> ToDashboardDtos(this IEnumerable<Category> entities)
        {
            if (entities is null) return [];
            return entities.Select(e => e.ToDashboardDto()).ToList();
        }

        public static DashboardCourseDto ToDashboardDto(this Course entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new DashboardCourseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Author = entity.Author,
                CourseLanguage = entity.CourseLanguage,
                CourseVideosCount = entity.CourseVideosCount,
                rating = entity.rating,
                Image = entity.ImageUrl,
                Category = entity.Category != null ? new CategoryResponseDto
                {
                    Id = entity.Category.Id,
                    Name = entity.Category.Name,
                    Description = entity.Category.Description,
                    ImageUrl = entity.Category.ImageUrl
                } : null,
                CreatedAt = entity.CreatedAt
            };
        }

        public static IReadOnlyList<DashboardCourseDto> ToDashboardDtos(this IEnumerable<Course> entities)
        {
            if (entities is null) return [];
            return entities.Select(e => e.ToDashboardDto()).ToList();
        }

        public static Category ToEntity(this CreateCategoryCommand command, string? imageUrl = null)
        {
            ArgumentNullException.ThrowIfNull(command);

            return new Category
            {
                Name = command.Name,
                Description = command.Description,
                ImageUrl = imageUrl
            };
        }

        public static Category ToEntity(this AdminDashboard.Dto.CreateCategoryDto dto, string? imageUrl = null)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new Category
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = imageUrl
            };
        }
    }
}
