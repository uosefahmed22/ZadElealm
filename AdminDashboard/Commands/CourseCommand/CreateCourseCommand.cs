using AdminDashboard.Models;
using MediatR;
using ZadElealm.Core.Errors;

namespace AdminDashboard.Commands.CourseCommand
{
    public class CreateCourseCommand : IRequest<ApiDataResponse>
    {
        public PlaylistViewModel playlistViewModel { get; set; }

        public CreateCourseCommand(PlaylistViewModel playlistViewModel)
        {
            this.playlistViewModel = playlistViewModel;
        }
    }
}
