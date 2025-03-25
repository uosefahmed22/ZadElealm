using AdminDashboard.Dto;
using AdminDashboard.Quires.CategoryQuery;
using AutoMapper;
using MediatR;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Dtos.DtosCategory;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Handlers.CategoryHandler
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, IReadOnlyList<DashboardCategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<DashboardCategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            return _mapper.Map<IReadOnlyList<Category>, IReadOnlyList<DashboardCategoryDto>>(categories);
        }
    }
}
