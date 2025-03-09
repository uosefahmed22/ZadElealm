using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;

namespace ZadElealm.Apis.Commands.Report
{
    public class AddReportCommand : BaseCommand<ApiResponse>
    {
        public string UserId { get; }
        public ReportDto ReportDto { get; }

        public AddReportCommand(string userId, ReportDto reportDto)
        {
            UserId = userId;
            ReportDto = reportDto;
        }
    }
}
