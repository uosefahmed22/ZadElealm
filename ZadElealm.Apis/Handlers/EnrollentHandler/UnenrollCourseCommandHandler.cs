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
            try
            {
                var enrollment = await _unitOfWork.Repository<Enrollment>()
                    .GetEntityWithSpecAsync(new EnrollmentSpecification(request.CourseId, request.UserId));

                if (enrollment == null)
                    return new ApiResponse(404, "لم يتم العثور على التسجيل");

                //_unitOfWork.Repository<Enrollment>().Delete(enrollment);
                
                enrollment.IsDeleted = true;
                _unitOfWork.Repository<Enrollment>().Update(enrollment);
                await _unitOfWork.Complete();

                return new ApiResponse(200, "تم إلغاء التسجيل بنجاح");
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, "حدث خطأ أثناء إلغاء التسجيل");
            }
        }
    }
}
