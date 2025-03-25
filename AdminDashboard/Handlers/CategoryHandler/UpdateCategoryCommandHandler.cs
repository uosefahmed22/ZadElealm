using AdminDashboard.Commands.CategoryCommand;
using AdminDashboard.Dto;
using AutoMapper;
using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;

namespace AdminDashboard.Handlers.CategoryHandler
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;
        private readonly IMapper _mapper;

        public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork, IImageService imageService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
            _mapper = mapper;
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
                category.ImageUrl = uploadedImage.Data as string;
            }

            var mappedCategory = _mapper.Map<CreateCategoryDto>(category);

            await _unitOfWork.Complete();
            return true;
        }
    }
}
