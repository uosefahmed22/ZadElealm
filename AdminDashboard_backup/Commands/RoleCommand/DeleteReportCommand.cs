using MediatR;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Commands.RoleCommand
{
    public class DeleteReportCommand : IRequest<ApiDataResponse>
    {
        public int ReportId { get; set; }
    }
}
