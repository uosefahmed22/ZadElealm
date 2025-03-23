using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Service;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Service.AppServices
{
    public class ImageService : IImageService
    {
        // Define allowed image file extensions
        private readonly List<string> _allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
        private readonly Cloudinary _cloudinary;

        public ImageService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<ApiDataResponse> UploadImageAsync(IFormFile imageFile)
        {
            // Validate if file exists
            if (imageFile == null || imageFile.Length == 0|| imageFile.Length > 5 * 1024 * 1024)
            {
                return new ApiDataResponse(400, null, "لا يمكن رفع الملف");
            }

            // Validate file extension
            var ext = Path.GetExtension(imageFile.FileName).ToLower();
            if (!_allowedExtensions.Contains(ext))
            {
                return new ApiDataResponse(400, null, "امتداد الملف غير مدعوم");
            }

            // Configure and execute upload to Cloudinary
            using var stream = imageFile.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(imageFile.FileName, stream),
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            // Return response based on upload result
            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new ApiDataResponse(200, uploadResult.Url.AbsoluteUri, "تم رفع الصورة بنجاح");
            }
            else
            {
                return new ApiDataResponse(400, null, "فشل في رفع الصورة");
            }
        }

        // Helper method to extract public ID from Cloudinary URL
        private string GetPublicIdFromUrl(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url), "الرابط لا يمكن ان يكون فارغاً");
            }

            try
            {
                var uri = new Uri(url);
                var segments = uri.Segments;
                var publicId = segments.Last().Split('.').First();
                return publicId;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("فشل في استخراج الرقم العام من الرابط");
            }
        }

        public async Task<ApiDataResponse> DeleteImageAsync(string imageUrl)
        {
            // Validate image URL
            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentNullException(nameof(imageUrl), "الرابط لا يمكن ان يكون فارغا");
            }

            // Get public ID and validate
            var publicId = GetPublicIdFromUrl(imageUrl);
            if (string.IsNullOrEmpty(publicId))
            {
                return new ApiDataResponse(400, null, "فشل في حذف الصورة");
            }

            // Configure and execute deletion
            var deletionParams = new DeletionParams(publicId)
            {
                Invalidate = true
            };

            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            // Return response based on deletion result
            if (deletionResult.StatusCode == HttpStatusCode.OK)
            {
                return new ApiDataResponse(200, null, "تم حذف الصورة بنجاح");
            }
            else
            {
                return new ApiDataResponse(400, null, "فشل في حذف الصورة");
            }
        }
    }
}