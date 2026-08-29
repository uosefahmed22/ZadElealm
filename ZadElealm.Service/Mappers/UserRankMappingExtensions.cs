using System;
using System.Collections.Generic;
using System.Linq;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models;

namespace ZadElealm.Service.Mappers
{
    public static class UserRankMappingExtensions
    {
        public static UserRankDto ToDto(this UserRank entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new UserRankDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                UserName = entity.User?.DisplayName,
                UserImage = entity.User?.ImageUrl,
                TotalPoints = entity.TotalPoints,
                Rank = entity.Rank,
                CompletedCoursesCount = entity.CompletedCoursesCount,
                CertificatesCount = entity.CertificatesCount,
                AverageQuizScore = entity.AverageQuizScore,
                LastUpdated = entity.LastUpdated
            };
        }

        public static List<UserRankDto> ToDtos(this IEnumerable<UserRank> entities)
        {
            if (entities is null) return [];
            return entities.Select(e => e.ToDto()).ToList();
        }
    }
}
