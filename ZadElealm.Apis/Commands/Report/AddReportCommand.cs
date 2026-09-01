using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Errors;

namespace ZadElealm.Apis.Commands.Report
{
    public class AddReportCommand : BaseCommand<ApiResponse>
    {
        public string UserId { get; }
        public CreateReportDto ReportDto { get; }

        public AddReportCommand(string userId, CreateReportDto reportDto)
        {
            UserId = userId;
            ReportDto = reportDto;
        }
    }
}
