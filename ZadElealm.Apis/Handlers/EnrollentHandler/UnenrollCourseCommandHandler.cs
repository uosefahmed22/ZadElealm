using Microsoft.Extensions.Caching.Memory;
using ZadElealm.Apis.Commands.EnrollmentCommands;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Handlers.EnrollentHandler
{
    public class UnenrollCourseCommandHandler : BaseCommandHandler<UnenrollCourseCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UnenrollCourseCommandHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(UnenrollCourseCommand request, CancellationToken cancellationToken)
        {
            var enrollment = await _unitOfWork.Repository<Enrollment>()
                .GetEntityWithSpecAsync(new EnrollmentSpecification(request.CourseId, request.UserId));

            if (enrollment == null)
                return new ApiResponse(404, "لم يتم العثور على التسجيل");

            enrollment.IsDeleted = true;
            await _unitOfWork.Complete();

            return new ApiResponse(200, "تم إلغاء التسجيل بنجاح");
        }
    }
}