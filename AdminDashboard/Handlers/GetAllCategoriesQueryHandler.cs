using AdminDashboard.Dto;
using AdminDashboard.Quires;
using AutoMapper;
using MediatR;
using ZadElealm.Apis.Dtos.DtosCategory;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Handlers
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, IReadOnlyList<CategoryResponseDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<CategoryResponseDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();
            return _mapper.Map<IReadOnlyList<Category>, IReadOnlyList<CategoryResponseDto>>(categories);
        }
    }
}
