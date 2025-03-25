using AdminDashboard.Commands.CategoryCommand;
using AutoMapper;
using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;

namespace AdminDashboard.Handlers
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public CreateCategoryCommandHandler(IUnitOfWork unitOfWork, IImageService imageService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            _mapper = mapper;
        }

        public async Task<bool> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var uploadedImage = await _imageService.UploadImageAsync(request.ImageUrl);
            var category = _mapper.Map<Category>(request);
            category.ImageUrl = uploadedImage.Data as string;

            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.Complete();

            return true;
        }
    }
}
