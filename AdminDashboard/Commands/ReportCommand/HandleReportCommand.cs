using MediatR;
using ZadElealm.Apis.Commands;
using ZadElealm.Core.Errors;

namespace AdminDashboard.Commands.ReportCommand
{
    public class HandleReportCommand : IRequest<ApiDataResponse> 
    {
        public int ReportId { get; set; }
        public string AdminResponse { get; set; }
    }
}
