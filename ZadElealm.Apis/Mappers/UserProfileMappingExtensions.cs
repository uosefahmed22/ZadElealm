using System;
using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Apis.Mappers
{
    public static class UserProfileMappingExtensions
    {
        public static UserProfileDTO ToProfileDto(this AppUser entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new UserProfileDTO
            {
                Id = entity.Id,
                DisplayName = entity.DisplayName,
                Email = entity.Email,
                ImageUrl = entity.ImageUrl,
                UserName = entity.UserName,
                PhoneNumber = entity.PhoneNumber
            };
        }
    }
}
