using AdminDashboard.Commands.CourseCommand;
using AdminDashboard.Dto;
using AutoMapper;
using MediatR;
using System.Net;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;

namespace AdminDashboard.Handlers.CourseHandler
{
    public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageService _imageService;

        public UpdateCourseCommandHandler(IUnitOfWork unitOfWork, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _imageService = imageService;
        }

        public async Task<bool> Handle(UpdateCourseCommand command, CancellationToken cancellationToken)
        {
            var course = await _unitOfWork.Repository<Course>().GetEntityAsync(command.CourseDto.Id);
            if (course == null)
                return false;

            course.Name = command.CourseDto.Name;
            course.Description = command.CourseDto.Description;
            course.Author = command.CourseDto.Author;
            course.CourseLanguage = command.CourseDto.CourseLanguage;

            if (command.CourseDto.formFile != null)
            {
                var uploadedImage = await _imageService.UploadImageAsync(command.CourseDto.formFile);
                course.ImageUrl = uploadedImage.Data as string;
            }

            await _unitOfWork.Complete();
            return true;
        }
    }
}