using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Core.Service
{
    public interface IImageService
    {
        Task<ApiDataResponse> UploadImageAsync(IFormFile imageFile);
        Task<ApiDataResponse> DeleteImageAsync(string imageUrl);
    }
}
