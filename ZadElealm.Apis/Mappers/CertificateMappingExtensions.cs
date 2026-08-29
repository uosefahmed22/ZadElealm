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

            return new CertificateDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                PdfUrl = entity.PdfUrl,
                CompletedDate = entity.CreatedAt,
                UserName = entity.User?.DisplayName,
                QuizName = entity.Quiz?.Name
            };
        }

        public static IReadOnlyList<CertificateDto> ToDtos(this IEnumerable<Certificate> entities)
        {
            if (entities is null) return [];
            return entities.Select(e => e.ToDto()).ToList();
        }
    }
}
