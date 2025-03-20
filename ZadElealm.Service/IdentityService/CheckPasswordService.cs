using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Service.IdentityService
{
    public class CheckPasswordService : ICheckPasswordService
    {
        private readonly UserManager<AppUser> _userManager;

        public CheckPasswordService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse> CheckPasswordAsync(string Email, string password)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(Email);
                var result = await _userManager.CheckPasswordAsync(user, password);
                if (!result)
                {
                    return new ApiResponse(401, "Invalid password");
                }

                return new ApiResponse(200, "Password verified successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "An error occurred");
            }
        }
    }
}
