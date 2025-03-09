using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Repositories;

namespace ZadElealm.Apis.Controllers
{
    public class RatingController : ApiBaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public RatingController(IUnitOfWork unitOfWork,IMapper mapper,UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }
        
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
        [HttpPost]
        public async Task<IActionResult> AddRating(int value, int courseId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return Unauthorized(new ApiResponse(401, "User not found"));
            }
            var existingCourse = await _unitOfWork.Repository<Course>().GetByIdAsync(courseId);
            if (existingCourse == null)
            {
                return BadRequest(new ApiResponse(404, "Course not found"));
            }

            var rating = new Rating
            {
                Value = value,
                courseId = courseId,
                UserId = user.Id
            };

            await _unitOfWork.Repository<Rating>().AddAsync(rating);
            await _unitOfWork.Complete();
            return Ok(new ApiResponse(200, "Rating added successfully"));
        }
    }
}
