using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Controllers
{
    public class AccountController : ApiBaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly ISendEmailService _sendEmailService;
        private readonly IOtpService _otpService;
        private readonly IMemoryCache _cache;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager,
            ITokenService tokenService,
            ISendEmailService sendEmailService,
            IOtpService otpService,
            IMemoryCache cache,
            RoleManager<IdentityRole> roleManager,
            SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _sendEmailService = sendEmailService;
            _otpService = otpService;
            _cache = cache;
            _roleManager = roleManager;
            _signInManager = signInManager;

        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null)
            {
                return Unauthorized(new ApiResponse(401));
            }

            if (!user.EmailConfirmed)
            {
                return Unauthorized(new ApiResponse(401, "Email not confirmed"));
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new ApiResponse(401));
            }

            return await _tokenService.CreateToken(user);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDTO registerDTO)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDTO.Email);
            if (existingUser != null)
            {
                return BadRequest(new ApiResponse(400, "Email is already in use"));
            }
            var user = new AppUser
            {
                DisplayName = registerDTO.DisplayName,
                Email = registerDTO.Email,
                UserName = registerDTO.Email.Split('@')[0]
            };

            var createUserResult = await _userManager.CreateAsync(user, registerDTO.Password);
            if (!createUserResult.Succeeded)
            {
                return BadRequest(new ApiResponse(400, "User creation failed"));
            }
            await _userManager.AddToRoleAsync(user, "User");

            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callBackUrl = GenerateCallBackUrl(emailConfirmationToken, user.Id);

            var emailBody = $"<h1>Dear {user.DisplayName}</h1>" +
                            $"<p>To confirm your email address, click the link below:</p>" +
                            $"<p><a href='{callBackUrl}'>Click here</a></p>";

            try
            {
                await _sendEmailService.SendEmailAsync(new EmailMessage{To = user.Email,Subject = "Confirm your email address",Body = emailBody});
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, "Failed to send confirmation email"));
            }
            return Ok(new ApiResponse(200, "User created successfully, please check your email for confirmation link"));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("current-user")]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            return await _tokenService.CreateToken(user);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse>> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                return Unauthorized(new ApiResponse(401));
            }
            var user = await _userManager.FindByEmailAsync(email);
            var result = await _userManager.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse(400, "Password is incorrect"));
            }
            return Ok(new ApiResponse(200, "Password changed successfully"));
        }

        [HttpPost("forget-password")]
        public async Task<ActionResult<ApiResponse>> ForgetPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(new ApiResponse(400, "Email not found."));
            }

            try
            {
                var otp = _otpService.GenerateOtp(email);
                var body = $"<h1>Your OTP is {otp}</h1>";

                await _sendEmailService.SendEmailAsync(new EmailMessage { To = email, Subject = "Reset your password", Body = body });

                return Ok(new ApiResponse(200, "OTP sent successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse(500, "An error occurred while sending the OTP."));
            }
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<ApiResponse>> VerifyOtp(string email, string otp)
        {
            try
            {
                var isValidOtp = await Task.Run(() => _otpService.IsValidOtp(email, otp));
                if (!isValidOtp)
                {
                    return BadRequest(new ApiResponse(400, "Invalid credentials."));
                }
                return Ok(new ApiResponse(200, "OTP verified successfully."));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying OTP: {ex.Message}");
                return StatusCode(500, new ApiResponse(500, "An error occurred while verifying OTP."));
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse>> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordDTO.Email);
                if (user == null)
                {
                    return BadRequest(new ApiResponse(400, "User not found."));
                }

                if (!_cache.TryGetValue(resetPasswordDTO.Email, out bool isOtpValid) || !isOtpValid)
                {
                    return BadRequest(new ApiResponse(400, "Invalid or expired OTP."));
                }
                _cache.Remove(resetPasswordDTO.Email);

                var isOldPasswordEqualNew = await _userManager.CheckPasswordAsync(user, resetPasswordDTO.NewPassword);
                if (isOldPasswordEqualNew)
                {
                    return BadRequest(new ApiResponse(400, "The new password cannot be the same as the old password."));
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, resetPasswordDTO.NewPassword);

                if (resetResult.Succeeded)
                {
                    return Ok(new ApiResponse(200, "Password has been successfully reset."));
                }

                var errorMessages = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                return StatusCode(500, new ApiResponse(500, $"Failed to reset password: {errorMessages}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new ApiResponse(500, "An error occurred while processing your request."));
            }
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<ActionResult<ApiResponse>> ResendConfirmationEmail([EmailAddress] string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(new ApiResponse(404, "User not found."));
                }
                if (user.EmailConfirmed)
                {
                    return BadRequest(new ApiResponse(400, "Email is already confirmed."));
                }

                var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callBackUrl = GenerateCallBackUrl(emailConfirmationToken, user.Id);
                var emailBody = $"<h1>Dear {user.DisplayName}" +
                    $"</h1><p>To confirm your email address, click the link below:</p>" +
                    $"</p><a href='{callBackUrl}'>click here</a></p>";

                await _sendEmailService.SendEmailAsync(new EmailMessage { To = user.Email, Subject = "Confirm your email address", Body = emailBody });

                return Ok(new ApiResponse(200, "Confirmation email sent successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse(500, "An error occurred while processing your request."));
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequest)
        {
            var result = await _tokenService.RefreshToken(tokenRequest.Token, tokenRequest.RefreshToken);
            return Ok(result);
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] TokenRequestDto tokenRequest)
        {
            var result = await _tokenService.RevokeToken(tokenRequest.Token, tokenRequest.RefreshToken);
            return Ok(result);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string confirmationToken)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse(404, "User not found."));
            }
            if (user.EmailConfirmed)
            {
                return BadRequest(new ApiResponse(400, "Email is already confirmed."));
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmationToken);
            if (result.Succeeded)
            {
                return RedirectPermanent(@"https://www.google.com/webhp?authuser=0");
            }
            else
            {
                return BadRequest(new ApiResponse(400, "Failed to confirm email. Invalid or expired token."));
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost("add-role")]
        public async Task<IActionResult> createRole(string role)
        {
            var roleExist = await _roleManager.RoleExistsAsync(role);
            if (!roleExist)
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
                return Ok(new ApiResponse(200, "Role created successfully."));
            }
            return BadRequest(new ApiResponse(400, "Role already exists."));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost("add-user-to-role")]
        public async Task<IActionResult> AddUserToRole(string email, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new ApiResponse(404, "User not found."));
            }
            var roleExist = await _roleManager.RoleExistsAsync(role);
            if (!roleExist)
            {
                return NotFound(new ApiResponse(404, "Role not found."));
            }
            var result = await _userManager.AddToRoleAsync(user, role);
            if (result.Succeeded)
            {
                return Ok(new ApiResponse(200, "User added to role successfully."));
            }
            return BadRequest(new ApiResponse(400, "Failed to add user to role."));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost("remove-user-from-role")]
        public async Task<IActionResult> RemoveUserFromRole(string email, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new ApiResponse(404, "User not found."));
            }
            var roleExist = await _roleManager.RoleExistsAsync(role);
            if (!roleExist)
            {
                return NotFound(new ApiResponse(404, "Role not found."));
            }
            var result = await _userManager.RemoveFromRoleAsync(user, role);
            if (result.Succeeded)
            {
                return Ok(new ApiResponse(200, "User removed from role successfully."));
            }
            return BadRequest(new ApiResponse(400, "Failed to remove user from role."));
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost("delete-user")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new ApiResponse(404, "User not found."));
            }
            user.IsDeleted = true;
            user.DeletedAt = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(new ApiResponse(200, "User deleted successfully."));
            }
            return BadRequest(new ApiResponse(400, "Failed to delete user."));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost("restore-user")]
        public async Task<IActionResult> RestoreUser(string email)
        {
            var user = await _userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !user.IsDeleted)
            {
                return NotFound(new ApiResponse(404, "User not found."));
            }
            user.IsDeleted = false;
            user.DeletedAt = null;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(new ApiResponse(200, "User restored successfully."));
            }
            return BadRequest(new ApiResponse(400, "Failed to restore user."));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users
                .Select(u => new UsersDtoResponse
                {
                    Id = u.Id,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    UserName = u.UserName
                }).ToListAsync();
            return Ok(users);
        }

        private string GenerateCallBackUrl(string token, string userId)
        {
            var encodedToken = Uri.EscapeDataString(token);
            var encodedUserId = Uri.EscapeDataString(userId);
            return $"{Request.Scheme}://{Request.Host}/api/Account/confirm-email?userId={encodedUserId}&confirmationToken={encodedToken}";
        }
    }
}
