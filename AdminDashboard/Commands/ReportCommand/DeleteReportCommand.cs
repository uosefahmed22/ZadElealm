using MediatR;
using ZadElealm.Core.Errors;

namespace AdminDashboard.Commands.ReportCommand
{
    public class DeleteReportCommand : IRequest<ApiDataResponse>
    {
        public int ReportId { get; set; }
    }
}
