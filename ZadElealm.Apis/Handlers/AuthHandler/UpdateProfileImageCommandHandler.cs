using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Errors;
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
            if (user == null)
                return new ApiResponse(404, "المستخدم غير موجود");

            if (request.File == null)
            {
                if (user.ImageUrl == null)
                    return new ApiResponse(404, "لا توجد صورة للحذف");

                if (!string.IsNullOrEmpty(user.ImageUrl))
                {
                    var deleteResult = await _imageService.DeleteImageAsync(user.ImageUrl);
                    if (deleteResult.StatusCode != 200)
                        return new ApiResponse(deleteResult.StatusCode, deleteResult.Message);
                    user.ImageUrl = null;
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                        return new ApiResponse(400, "تعذر تحديث صورة الملف الشخصي");
                }

                return new ApiResponse(200, "تم حذف صورة الملف الشخصي بنجاح");
            }

            var imageUrl = await _imageService.UploadImageAsync(request.File);
            if (imageUrl.StatusCode != 200 || imageUrl.Data is not string uploadedUrl)
                return new ApiResponse(imageUrl.StatusCode, imageUrl.Message ?? "فشل في رفع الصورة");

            var oldImageUrl = user.ImageUrl;
            user.ImageUrl = uploadedUrl;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                await _imageService.DeleteImageAsync(uploadedUrl);
                return new ApiResponse(400, "تعذر تحديث صورة الملف الشخصي");
            }

            if (!string.IsNullOrEmpty(oldImageUrl))
                await _imageService.DeleteImageAsync(oldImageUrl);

            return new ApiResponse(200, "تم رفع الصورة بنجاح");
        }
    }
}
