using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Auth;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Handlers.AuthHandler
{
    public class UpdateProfileImageCommandHandler : BaseCommandHandler<UpdateProfileImageCommand, ApiResponse>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;

        public UpdateProfileImageCommandHandler(
            UserManager<AppUser> userManager,
            IImageService imageService)
        {
            _userManager = userManager;
            _imageService = imageService;
        }

        public override async Task<ApiResponse> Handle(UpdateProfileImageCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);

            if (request.File == null)
            {
                if (user.ImageUrl == null)
                    return new ApiResponse(404, "لا توجد صورة للحذف");

                if (!string.IsNullOrEmpty(user.ImageUrl))
                {
                    await _imageService.DeleteImageAsync(user.ImageUrl);
                    user.ImageUrl = null;
                    await _userManager.UpdateAsync(user);
                }

                return new ApiResponse(200, "تم حذف صورة الملف الشخصي بنجاح");
            }

            if (!string.IsNullOrEmpty(user.ImageUrl))
            {
                await _imageService.DeleteImageAsync(user.ImageUrl);
            }

            var imageUrl = await _imageService.UploadImageAsync(request.File);
            user.ImageUrl = imageUrl.Data as string;
            await _userManager.UpdateAsync(user);

            return new ApiResponse(200, "تم رفع الصورة بنجاح");
        }
    }
}
