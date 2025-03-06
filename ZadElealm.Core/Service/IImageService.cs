using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZadElealm.Core.Service
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile imageFile);
        Task<string> DeleteImageAsync(string imageUrl);
    }
}
