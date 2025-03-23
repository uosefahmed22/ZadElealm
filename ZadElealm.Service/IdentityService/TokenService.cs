using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.Service.IdentityService
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly AppDbContext _dbContext;

        public TokenService(IConfiguration configuration,
            UserManager<AppUser> userManager,
            TokenValidationParameters tokenValidationParameters,
            AppDbContext dbContext)
        {
            _configuration = configuration;
            _userManager = userManager;
            _tokenValidationParameters = tokenValidationParameters;
            _dbContext = dbContext;
        }

        public async Task<UserDTO> CreateToken(AppUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:AccessTokenExpirationInMinutes"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                UserId = user.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                Token = GenerateRefreshToken()
            };

            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();

            return new UserDTO
            {
                DisplayName = user.DisplayName,
                Token = jwtToken,
                Email = user.Email,
                RefreshToken = refreshToken.Token
            };
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Base64UrlEncoder.Encode(randomNumber);
        }
        public async Task<AuthResult> RefreshToken(string Token, string RefreshToken)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                _tokenValidationParameters.ValidateLifetime = false;

                var tokenInVerification = jwtTokenHandler.ValidateToken(Token, _tokenValidationParameters, out var validatedToken);
                if (tokenInVerification == null)
                {
                    return new AuthResult { Result = false, message = "رمز غير صالح." };
                }

                var claims = tokenInVerification.Claims.ToDictionary(c => c.Type, c => c.Value);

                if (!claims.TryGetValue(JwtRegisteredClaimNames.Exp, out var expValue) ||
                    !long.TryParse(expValue, out var utcExpiryDate) ||
                    DateTimeOffset.FromUnixTimeSeconds(utcExpiryDate) > DateTime.UtcNow)
                {
                    return new AuthResult { Result = false, message = "لم تنته صلاحية هذا الرمز بعد." };
                }

                var storedToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == RefreshToken);
                if (storedToken == null || storedToken.Used || storedToken.IsRevoked || storedToken.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResult { Result = false, message = "رمز تحديث غير صالح." };
                }
                var user = await _userManager.FindByIdAsync(storedToken.UserId);
                if (user == null || !user.IsDeleted)
                {
                    return new AuthResult { Result = false, message = "رمز تحديث غير صالح." };
                }

                if (!claims.TryGetValue(JwtRegisteredClaimNames.Jti, out var jti) || storedToken.JwtId != jti)
                {
                    return new AuthResult { Result = false, message = "رمز غير صالح." };
                }

                storedToken.Used = true;
                _dbContext.RefreshTokens.Update(storedToken);
                await _dbContext.SaveChangesAsync();
                return new AuthResult { Result = true, message = "تم تحديث الرمز." };

            }
            catch (SecurityTokenException ex)
            {
                return new AuthResult { Result = false, message = ex.Message };
            }
        }
        public async Task<AuthResult> RevokeToken(string Token, string RefreshToken)
        {
            try
            {
                var storedToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == RefreshToken);
                if (storedToken == null)
                {
                    return new AuthResult
                    {
                        Result = false,
                        message = "رمز تحديث غير صالح."
                    };
                }

                if (storedToken.IsRevoked)
                {
                    return new AuthResult
                    {
                        Result = false,
                        message = "تم إلغاء الرمز بالفعل."
                    };
                }

                storedToken.IsRevoked = true;
                _dbContext.RefreshTokens.Update(storedToken);
                await _dbContext.SaveChangesAsync();

                return new AuthResult
                {
                    Result = true,
                    message = "تم إلغاء الرمز."
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Result = false,
                    message = ex.Message
                };
            }
        }
    }
}
