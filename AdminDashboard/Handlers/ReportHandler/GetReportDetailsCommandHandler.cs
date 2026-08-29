using AdminDashboard.Commands;
using AdminDashboard.Commands.ReportCommand;
using ZadElealm.Apis.Mappers;
using ZadElealm.Apis.Dtos;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Handlers;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Handlers.ReportHanlder
{
    public class GetReportDetailsCommandHandler : BaseCommandHandler<GetReportDetailsCommand, ApiDataResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetReportDetailsCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiDataResponse> Handle(GetReportDetailsCommand request, CancellationToken cancellationToken)
        {
            var report = await _unitOfWork.Repository<Report>().GetEntityWithNoTrackingAsync(request.Id);
            if (report == null)
                return new ApiDataResponse(404, null, "التقرير غير موجود");

            var mappedReport = report.ToDto();
            return new ApiDataResponse(200, mappedReport);
        }
    }
}
