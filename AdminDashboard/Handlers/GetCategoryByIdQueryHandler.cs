using AdminDashboard.Quires;
using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Handlers
{
    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Category>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Category> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Repository<Category>().GetEntityAsync(request.Id);
        }
    }
}
