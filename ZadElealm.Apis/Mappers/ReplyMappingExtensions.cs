using System;
using System.Collections.Generic;
using System.Linq;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models;

namespace ZadElealm.Apis.Mappers
{
    public static class ReplyMappingExtensions
    {
        public static ReplyDto ToDto(this Reply entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new ReplyDto
            {
                Id = entity.Id,
                Text = entity.Text,
                AppUserId = entity.User?.Id ?? entity.AppUserId,
                DisplayName = entity.User?.DisplayName,
                UserImage = entity.User?.ImageUrl,
                CreatedAt = entity.CreatedAt,
                ReplyLikesCount = entity.ReplyLikes != null ? entity.ReplyLikes.Count : entity.ReplyLikesCount
            };
        }

        public static IReadOnlyList<ReplyDto> ToDtos(this IEnumerable<Reply> entities)
        {
            if (entities is null) return [];
            return entities.Select(e => e.ToDto()).ToList();
        }
    }
}
