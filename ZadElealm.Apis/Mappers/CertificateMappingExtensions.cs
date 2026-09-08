using System;
using System.Collections.Generic;
using System.Linq;
using ZadElealm.Core.Models;
using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Apis.Mappers
{
    public static class CertificateMappingExtensions
    {
        public static CertificateDto ToDto(this Certificate entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var assessmentName = entity.Assessment?.Name ?? entity.Quiz?.Name ?? "الاختبار";

            return new CertificateDto
            {
                Id = entity.Id,
                Name = $"شهادة اجتياز {assessmentName}",
                Description = ToArabicDescription(entity.Description, assessmentName),
                PdfUrl = $"/api/Certificate/{entity.Id}/file",
                CompletedDate = entity.CreatedAt,
                UserName = entity.User?.DisplayName ?? string.Empty,
                QuizName = assessmentName
            };
        }

        public static IReadOnlyList<CertificateDto> ToDtos(this IEnumerable<Certificate> entities)
        {
            if (entities is null) return [];
            return entities.Select(e => e.ToDto()).ToList();
        }

        private static string ToArabicDescription(string? description, string assessmentName)
        {
            if (!string.IsNullOrWhiteSpace(description) &&
                !description.Any(character => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z'))
            {
                return description;
            }

            return $"شهادة إتمام {assessmentName} بنجاح";
        }
    }
}
