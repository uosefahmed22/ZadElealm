using AutoMapper;
using ZadElealm.Apis.Dtos.DtosCategory;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Category;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace ZadElealm.Apis.Handlers.Category
{
    public class GetCategoriesQueryHandler : BaseQueryHandler<GetCategoriesQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCategoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public override async Task<ApiResponse> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var categories = await _unitOfWork.Repository<ZadElealm.Core.Models.Category>().GetAllWithNoTrackingAsync();
                var mappedCategories = _mapper.Map<IReadOnlyList<ZadElealm.Core.Models.Category>, IReadOnlyList<CategoryResponseDto>>(categories);

                return new ApiDataResponse(200,mappedCategories);
            }
            catch
            {
                return new ApiResponse(500, "حدث خطأ أثناء جلب الفئات");
            }
        }
    }
}
