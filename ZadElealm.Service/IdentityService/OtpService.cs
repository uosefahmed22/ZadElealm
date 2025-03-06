using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Service;

namespace ZadElealm.Service.IdentityService
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<OtpService> _logger;
        private readonly IConfiguration _configuration;

        public OtpService(IMemoryCache cache, ILogger<OtpService> logger, IConfiguration configuration)
        {
            _cache = cache;
            _logger = logger;
            _configuration = configuration;
        }

        public string GenerateOtp(string email)
        {
            try
            {
                var key = KeyGeneration.GenerateRandomKey(32);
                StoreKeyInCache(email, key);
                var step = int.Parse(_configuration["OtpSettings:Step"]);
                var totp = new Totp(key, step: step);
                return totp.ComputeTotp();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate OTP for {Email}", email);
                throw;
            }
        }
        private void StoreKeyInCache(string email, byte[] key)
        {
            _cache.Set(email, key, TimeSpan.FromMinutes(double.Parse(_configuration["OtpSettings:KeyExpirationMinutes"])));
        }
        public bool IsValidOtp(string email, string otp)
        {
            try
            {
                var key = RetrieveKeyFromCache(email);
                if (key is null)
                {
                    _logger.LogWarning("No OTP key found for {Email}", email);
                    return false;
                }

                var step = int.Parse(_configuration["OtpSettings:Step"]);
                var totp = new Totp(key, step: step);
                var isValidOtp = totp.VerifyTotp(otp, out _, new VerificationWindow(1, 1));
                if (!isValidOtp)
                {
                    _logger.LogWarning("Invalid OTP for {Email}", email);
                    return false;
                }

                _cache.Remove(email);
                _cache.Set(email, true, TimeSpan.FromMinutes(double.Parse(_configuration["OtpSettings:ValidationExpirationMinutes"])));

                return isValidOtp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate OTP for {Email}", email);
                throw;
            }
        }
        private byte[]? RetrieveKeyFromCache(string email)
        {
            return _cache.TryGetValue(email, out byte[]? key) ? key : null;
        }
    }
}
