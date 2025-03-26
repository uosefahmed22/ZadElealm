using AdminDashboard.Commands.CourseCommand;
using MediatR;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Handlers.CourseHandler
{
    public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCourseCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteCourseCommand command, CancellationToken cancellationToken)
        {
            var course = await _unitOfWork.Repository<Course>().GetEntityAsync(command.Id);

            if (course == null)
                return false;

            _unitOfWork.Repository<Course>().Delete(course);
            await _unitOfWork.Complete();

            return true;
        }
    }
}
