using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.EnrollmentCommands;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.EnrollmentQuery;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Handlers.EnrollentHandler
{
    public class EnrollCourseCommandHandler : BaseCommandHandler<EnrollCourseCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public EnrollCourseCommandHandler(
            IUnitOfWork unitOfWork,
            IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(EnrollCourseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var course = await _unitOfWork.Repository<Core.Models.Course>().GetByIdAsync(request.CourseId);
                if (course == null)
                    return new ApiResponse(404, "الدورة غير موجودة");

                var existingEnrollment = await _unitOfWork.Repository<Enrollment>()
                    .GetEntityWithSpecAsync(new EnrollmentSpecification(request.CourseId, request.UserId));

                if (existingEnrollment != null)
                    return new ApiResponse(400, "أنت مسجل بالفعل في هذه الدورة");

                var enrollment = new Enrollment
                {
                    CourseId = request.CourseId,
                    UserId = request.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<Enrollment>().AddAsync(enrollment);
                await _unitOfWork.Complete();

                _cache.Remove($"enrolled_courses_{request.UserId}");

                return new ApiResponse(200, "تم التسجيل في الدورة بنجاح");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "حدث خطأ أثناء التسجيل في الدورة");
            }
        }
    }
}