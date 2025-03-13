using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.EnrollmentCommands;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.EnrollmentQuery;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.ServiceDto;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Handlers.EnrollentHandler
{
    public class EnrollCourseCommandHandler : BaseCommandHandler<EnrollCourseCommand, ApiResponse>
    {
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public EnrollCourseCommandHandler(
            INotificationService notificationService,
            IUnitOfWork unitOfWork,
            IMemoryCache cache)
        {
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public override async Task<ApiResponse> Handle(EnrollCourseCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var course = await _unitOfWork.Repository<Core.Models.Course>().GetEntityAsync(request.CourseId);
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

                await _notificationService.SendNotificationAsync(new NotificationServiceDto
                {
                    UserId = request.UserId,
                    Type = NotificationType.Enrollment,
                    Title = "تهانينا على التسجيل!",
                    Description = "نسأل الله أن يبارك لك في علمك وعملك. لقد تم تسجيلك بنجاح في الدورة. نتمنى لك رحلة علمية مليئة بالفائدة والنور. نسأل الله لك التوفيق والسداد."
                });

                return new ApiResponse(200, "تم التسجيل في الدورة بنجاح");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "حدث خطأ أثناء التسجيل في الدورة");
            }
        }
    }
}