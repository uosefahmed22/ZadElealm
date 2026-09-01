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
using ZadElealm.Core.Errors;

namespace ZadElealm.Service.AppServices
{
    public class ImageService : IImageService
    {
        // Define allowed image file extensions
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png"
        };

        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png"
        };
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
            if (!AllowedExtensions.Contains(ext) || !AllowedContentTypes.Contains(imageFile.ContentType))
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
            if (uploadResult.StatusCode == HttpStatusCode.OK && uploadResult.SecureUrl != null)
            {
                return new ApiDataResponse(200, uploadResult.SecureUrl.AbsoluteUri, "تم رفع الصورة بنجاح");
            }
            else
            {
                return new ApiDataResponse(400, null, "فشل في رفع الصورة");
            }
        }
        public async Task<ApiDataResponse> DeleteImageAsync(string imageUrl)
        {
            // Validate image URL
            if (string.IsNullOrWhiteSpace(imageUrl))
                return new ApiDataResponse(400, null, "الرابط لا يمكن ان يكون فارغا");

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
        // Helper method to extract public ID from Cloudinary URL
        private static string? GetPublicIdFromUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (!uri.Host.Equals("cloudinary.com", StringComparison.OrdinalIgnoreCase) &&
                 !uri.Host.EndsWith(".cloudinary.com", StringComparison.OrdinalIgnoreCase)))
                return null;

            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var uploadIndex = Array.FindIndex(segments,
                segment => segment.Equals("upload", StringComparison.OrdinalIgnoreCase));
            if (uploadIndex < 0 || uploadIndex == segments.Length - 1)
                return null;

            var assetSegments = segments.Skip(uploadIndex + 1).ToList();
            var versionIndex = assetSegments.FindIndex(segment =>
                segment.Length > 1 && segment[0] == 'v' && segment[1..].All(char.IsDigit));
            if (versionIndex >= 0)
                assetSegments = assetSegments.Skip(versionIndex + 1).ToList();

            if (assetSegments.Count == 0)
                return null;

            assetSegments[^1] = Path.GetFileNameWithoutExtension(assetSegments[^1]);
            return assetSegments.Any(string.IsNullOrWhiteSpace)
                ? null
                : string.Join('/', assetSegments);
        }
    }
}
