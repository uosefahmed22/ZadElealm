using AdminDashboard.Commands;
using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;

namespace AdminDashboard.Handlers
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;

        public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }

        public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _unitOfWork.Repository<Category>().GetEntityAsync(request.Id);
            if (category == null) return false;

            category.Name = request.Name;
            category.Description = request.Description;

            if (request.ImageUrl != null)
            {
                var uploadedImage = await _imageService.UploadImageAsync(request.ImageUrl);
                category.ImageUrl = uploadedImage;
            }

            await _unitOfWork.Complete();
            return true;
        }
    }
}
