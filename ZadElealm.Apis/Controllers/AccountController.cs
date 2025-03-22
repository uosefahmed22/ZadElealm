using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ZadElealm.Apis.Commands.Auth;
using ZadElealm.Apis.Dtos.Auth;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Auth;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Controllers
{
    public class AccountController : ApiBaseController
    {
        private readonly IMediator _mediator;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;

        public AccountController(IMediator mediator, UserManager<AppUser> userManager, IImageService imageService)
        {
            _mediator = mediator;
            _userManager = userManager;
            _imageService = imageService;
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
        {
            var command = new LoginCommand(loginDTO);
            var result = await _mediator.Send(command);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse>> Register(RegisterDTO registerDTO)
        {
            var command = new RegisterCommand(registerDTO);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("current-user")]
        public async Task<ActionResult<ApiResponse>> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var query = new GetCurrentUserQuery(user.Id);
            var response = await _mediator.Send(query);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse>> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                 return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new ChangePasswordCommand(email, changePasswordDTO);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("update-profile-image")]
        public async Task<ActionResult<ApiResponse>> UpdateProfileImage(IFormFile? file)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new UpdateProfileImageCommand(user.Id, file);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpGet("get-User-Profile")]
        public async Task<ActionResult<ApiResponse>> GetProfile()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var query = new GetUserProfileQuery(user.Id);
            var response = await _mediator.Send(query);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("send-email-otp")]
        public async Task<ActionResult<ApiResponse>> SendEmailOtp([FromBody] SendChangeEmailOtpDto sendChangeEmailOtpDto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new SendChangeEmailOtpCommand(email, sendChangeEmailOtpDto.Password, sendChangeEmailOtpDto.NewEmail);
            var response = await _mediator.Send(command);
           
            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("update-email")]
        public async Task<ActionResult<ApiResponse>> UpdateEmail([FromBody] UpdateEmailDto request)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

                var command = new UpdateEmailCommand(user.Id, request.NewEmail, request.Token);
                var response = await _mediator.Send(command);

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse(400, "An error occurred"));
            }
        }
       
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost("update-profile")]
        public async Task<ActionResult<ApiResponse>> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                 return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new UpdateProfileCommand(request.DisplayName, request.PhoneNumber, email);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("forget-password")]
        public async Task<ActionResult<ApiResponse>> ForgetPassword(string email)
        {
            var command = new ForgetPasswordCommand(email);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("delete-account")]
        public async Task<ActionResult<ApiResponse>> DeleteAccount([FromBody] string password)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new ApiResponse(401, "المستخدم غير موجود"));

            var command = new DeleteAccountCommand( email, password);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<ApiResponse>> VerifyOtp(string email, string otp)
        {
            var command = new VerifyOtpCommand(email, otp);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse>> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var command = new ResetPasswordCommand(resetPasswordDTO);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<ActionResult<ApiResponse>> ResendConfirmationEmail([EmailAddress] string email)
        {
            var command = new ResendConfirmationEmailCommand(email);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse>> RefreshToken([FromBody] TokenRequestDto tokenRequest)
        {
            var command = new RefreshTokenCommand(tokenRequest);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("revoke-token")]
        public async Task<ActionResult<ApiResponse>> RevokeToken([FromBody] TokenRequestDto tokenRequest)
        {
            var command = new RevokeTokenCommand(tokenRequest);
            var response = await _mediator.Send(command);

            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var query = new ConfirmEmailQuery(userId, token);
            var response = await _mediator.Send(query);

            if (response.StatusCode == 200 && response.Message != null)
                return Redirect(response.Message);

            return BadRequest(response);
        }
    }
}
