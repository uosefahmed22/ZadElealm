using System;
using System.Collections.Generic;
using System.Linq;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;

namespace ZadElealm.Apis.Mappers
{
    public static class ReportMappingExtensions
    {
        public static Report ToEntity(this ReportDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new Report
            {
                Id = dto.Id ?? 0,
                TitleOfTheIssue = dto.TitleOfTheIssue,
                Description = dto.Description,
                reportTypes = Enum.TryParse<ReportType>(dto.reportTypes, true, out var parsedType) ? parsedType : ReportType.Other,
                AdminResponse = dto.AdminResponse,
                IsSolved = dto.IsSolved ?? false
            };
        }

        public static ReportDto ToDto(this Report entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new ReportDto
            {
                Id = entity.Id,
                TitleOfTheIssue = entity.TitleOfTheIssue,
                Description = entity.Description,
                reportTypes = entity.reportTypes.ToString(),
                AdminResponse = entity.AdminResponse,
                IsSolved = entity.IsSolved
            };
        }

        public static IReadOnlyList<ReportDto> ToDtos(this IEnumerable<Report> entities)
        {
            if (entities is null) return [];
            return entities.Select(e => e.ToDto()).ToList();
        }
    }
}
