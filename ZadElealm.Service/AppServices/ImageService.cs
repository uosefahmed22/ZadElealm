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
        private readonly List<string> _allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png" };
        private readonly Cloudinary _cloudinary;

        public ImageService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<ApiDataResponse> UploadImageAsync(IFormFile imageFile)
        {
            if (imageFile == null)
            {
                throw new ArgumentNullException(nameof(imageFile), "Image file cannot be null.");
            }

            if (imageFile.Length == 0)
            {
                return new ApiDataResponse(400, null, "File is empty.");
            }

            if (imageFile.Length > 5 * 1024 * 1024)
            {
                return new ApiDataResponse(400, null, "File size cannot exceed 5MB.");
            }

            var ext = Path.GetExtension(imageFile.FileName).ToLower();
            if (!_allowedExtensions.Contains(ext))
            {
                return new ApiDataResponse(400, null, "Invalid file type. Only JPG and PNG are allowed.");
            }

            try
            {
                using var stream = imageFile.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(imageFile.FileName, stream),
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new ApiDataResponse(200, uploadResult.Url.AbsoluteUri, "Image uploaded successfully.");
                }
                else
                {
                    return new ApiDataResponse(400, null, "Failed to upload image.");
                }
            }
            catch (Exception ex)
            {
                return new ApiDataResponse(500, null, $"An unexpected error occurred");
            }
        }
        private string GetPublicIdFromUrl(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url), "URL cannot be null.");
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
                throw new InvalidOperationException("Failed to extract public ID from URL.", ex);
            }
        }
        public async Task<ApiDataResponse> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentNullException(nameof(imageUrl), "Image URL cannot be null or empty.");
            }

            var publicId = GetPublicIdFromUrl(imageUrl);
            if (string.IsNullOrEmpty(publicId))
            {
                return new ApiDataResponse(400, null, "Failed to extract public ID from URL.");
            }

            try
            {
                var deletionParams = new DeletionParams(publicId)
                {
                    Invalidate = true
                };

                var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

                if (deletionResult.StatusCode == HttpStatusCode.OK)
                {
                    return new ApiDataResponse(200, null, "Image deleted successfully.");
                }
                else
                {
                    return new ApiDataResponse(400, null, "Failed to delete image.");
                }
            }
            catch (Exception ex)
            {
                return new ApiDataResponse(500, null, $"An unexpected error occurred");
            }
        }
    }
}
