using AdminDashboard.Commands.CategoryCommand;
using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Handlers.CategoryHandler
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _unitOfWork.Repository<Category>().GetEntityAsync(request.Id);
            if (category == null)
                return false;

            _unitOfWork.Repository<Category>().Delete(category);
            await _unitOfWork.Complete();

            return true;
        }
    }
}
