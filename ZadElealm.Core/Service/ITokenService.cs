using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Service
{
    public interface ITokenService
    {
        Task<UserDTO> CreateToken(AppUser user);
        Task<AuthResult> RefreshToken(string Token, string RefreshToken);
        Task<AuthResult> RevokeToken(string Token, string RefreshToken);
    }
}
