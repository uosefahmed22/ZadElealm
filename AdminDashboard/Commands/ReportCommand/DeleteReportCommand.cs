using MediatR;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Commands.ReportCommand
{
    public class DeleteReportCommand : IRequest<ApiDataResponse>
    {
        public int ReportId { get; set; }
    }
}
