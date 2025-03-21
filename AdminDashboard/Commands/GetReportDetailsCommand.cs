using ZadElealm.Apis.Commands;
using ZadElealm.Apis.Errors;

namespace AdminDashboard.Commands
{
    public class GetReportDetailsCommand : BaseCommand<ApiDataResponse>
    {
        public int Id { get; set; }
    }

}
