using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Core.Service
{
    public interface ICheckPasswordService
    {
        Task<ApiResponse> CheckPasswordAsync(string password, string Email);
    }
}
