using AdminDashboard.Commands.CategoryCommand;
using AdminDashboard.Mappers;
using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;

namespace AdminDashboard.Handlers.CategoryHandler
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;

        public CreateCategoryCommandHandler(IUnitOfWork unitOfWork, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }

        public async Task<bool> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var uploadedImage = await _imageService.UploadImageAsync(request.ImageUrl);
            var imageUrl = uploadedImage.Data as string;
            var category = request.ToEntity(imageUrl);

            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.Complete();

            return true;
        }
    }
}
