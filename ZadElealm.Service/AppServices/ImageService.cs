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

        public async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            if (imageFile == null)
            {
                throw new ArgumentNullException(nameof(imageFile), "Image file cannot be null.");
            }

            if (imageFile.Length == 0)
            {
                return "No file uploaded.";
            }

            if (imageFile.Length > 5 * 1024 * 1024)
            {
                return "File size should not exceed 5MB.";
            }

            var ext = Path.GetExtension(imageFile.FileName).ToLower();
            if (!_allowedExtensions.Contains(ext))
            {
                return $"Only {string.Join(", ", _allowedExtensions)} extensions are allowed.";
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
                    return uploadResult.SecureUrl.AbsoluteUri;
                }
                else
                {
                    return $"Error occurred while uploading the image. Status Code: {uploadResult.StatusCode}, Error: {uploadResult.Error?.Message}";
                }
            }
            catch (Exception ex)
            {
                return $"An unexpected error occurred: {ex.Message}";
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
        public async Task<string> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentNullException(nameof(imageUrl), "Image URL cannot be null or empty.");
            }

            var publicId = GetPublicIdFromUrl(imageUrl);
            if (string.IsNullOrEmpty(publicId))
            {
                return "Invalid image URL.";
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
                    return "Image deleted successfully.";
                }
                else
                {
                    return $"Failed to delete image. Status Code: {deletionResult.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"An unexpected error occurred: {ex.Message}";
            }
        }
    }
}
