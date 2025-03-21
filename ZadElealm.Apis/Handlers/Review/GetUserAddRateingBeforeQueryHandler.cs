using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Review;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.Rating;

namespace ZadElealm.Apis.Handlers.Review
{
    public class GetUserAddRateingBeforeQueryHandler : BaseQueryHandler<GetUserAddRateingBeforeQuery, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserAddRateingBeforeQueryHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<bool> Handle(GetUserAddRateingBeforeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var spec = new RatingSpecification(request.UserId, request.CourseId);
                var rating = await _unitOfWork.Repository<Core.Models.Rating>()
                    .GetEntityWithSpecNoTrackingAsync(spec);

                if (rating != null)
                    return false;

                return true;
            }
            catch (Exception)
            {
                throw new Exception("حدث خطأ أثناء التحقق من إضافة التقييم");
            }
        }
    }
}