using System;
using System.Collections.Generic;
using System.Linq;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Models;
using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Apis.Mappers
{
    public static class QuizMappingExtensions
    {
        public static QuizResponseDto ToResponseDto(this Quiz entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new QuizResponseDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                CreatedAt = entity.CreatedAt,
                Course = entity.Course?.ToDto(),
                Questions = entity.Questions != null
                    ? entity.Questions.Select(q => q.ToDto()).ToList()
                    : new List<QuestionDto>()
            };
        }

        public static QuestionDto ToDto(this Question entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new QuestionDto
            {
                Id = entity.Id,
                Text = entity.Text,
                QuizId = entity.QuizId,
                Choices = entity.Choices != null
                    ? entity.Choices.Select(c => c.ToDto()).ToList()
                    : new List<ChoiceDto>()
            };
        }

        public static IReadOnlyList<QuestionDto> ToDtos(this IEnumerable<Question> entities)
        {
            if (entities is null) return [];
            return entities.Select(e => e.ToDto()).ToList();
        }

        public static ChoiceDto ToDto(this Choice entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new ChoiceDto
            {
                Id = entity.Id,
                Text = entity.Text
            };
        }

        public static IReadOnlyList<ChoiceDto> ToDtos(this IEnumerable<Choice> entities)
        {
            if (entities is null) return [];
            return entities.Select(e => e.ToDto()).ToList();
        }
    }
}
