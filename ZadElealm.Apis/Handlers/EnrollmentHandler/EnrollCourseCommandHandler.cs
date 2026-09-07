using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.EnrollmentCommands;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Quaries.EnrollmentQuery;
using ZadElealm.Core.Enums;
using ZadElealm.Core.Models;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Handlers.EnrollentHandler
{
    public class EnrollCourseCommandHandler : BaseCommandHandler<EnrollCourseCommand, ApiResponse>
    {
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEnrollmentWriteRepository _enrollmentWriteRepository;

        public EnrollCourseCommandHandler(
            INotificationService notificationService,
            IUnitOfWork unitOfWork,
            IEnrollmentWriteRepository enrollmentWriteRepository)
        {
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
            _enrollmentWriteRepository = enrollmentWriteRepository;
        }

        public override async Task<ApiResponse> Handle(EnrollCourseCommand request, CancellationToken cancellationToken)
        {
            var course = await _unitOfWork.Repository<Core.Models.Course>().GetEntityAsync(request.CourseId);
            if (course == null)
                return new ApiResponse(404, "الدورة غير موجودة");

            var existingEnrollment = await _enrollmentWriteRepository.FindIncludingDeletedAsync(
                request.CourseId,
                request.UserId,
                cancellationToken);

            if (existingEnrollment is { IsDeleted: false })
                return new ApiResponse(400, "أنت مسجل بالفعل في هذه الدورة");

            var isRestored = existingEnrollment != null;
            var enrollment = existingEnrollment ?? new Enrollment
            {
                CourseId = request.CourseId,
                AppUserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            if (isRestored)
            {
                enrollment.IsDeleted = false;
                enrollment.UnenrolledAtUtc = null;
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (isRestored)
                    _unitOfWork.Repository<Enrollment>().Update(enrollment);
                else
                    await _unitOfWork.Repository<Enrollment>().AddAsync(enrollment);

                var notificationResult = await _notificationService.AddNotificationAsync(new NotificationServiceDto
                {
                    UserId = request.UserId,
                    Type = NotificationType.Enrollment,
                    Title = isRestored
                        ? $"تمت استعادة تسجيلك في «{course.Name}»"
                        : $"تم تسجيلك في «{course.Name}»",
                    Description = isRestored
                        ? $"تمت استعادة دورة «{course.Name}» مع الاحتفاظ بتقدمك السابق."
                        : $"تم تسجيلك بنجاح في دورة «{course.Name}». نسأل الله أن يبارك لك في علمك وعملك، ونتمنى لك رحلة علمية مليئة بالفائدة والنور."
                });
                if (notificationResult.StatusCode != 200)
                    throw new InvalidOperationException("Failed to add the enrollment notification.");

                await _unitOfWork.Complete();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return new ApiResponse(200, isRestored
                ? "تمت استعادة التسجيل مع الاحتفاظ بتقدمك"
                : "تم التسجيل في الدورة بنجاح");
        }
    }
}
