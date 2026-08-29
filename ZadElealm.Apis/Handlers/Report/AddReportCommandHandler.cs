using ZadElealm.Apis.Commands.Report;
using ZadElealm.Apis.Mappers;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Repositories;

namespace ZadElealm.Apis.Handlers.Report
{
    public class AddReportCommandHandler : BaseCommandHandler<AddReportCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddReportCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(AddReportCommand request, CancellationToken cancellationToken)
        {
            var mappedReport = request.ReportDto.ToEntity();
            mappedReport.AppUserId = request.UserId;

            await _unitOfWork.Repository<Core.Models.Report>().AddAsync(mappedReport);
            await _unitOfWork.Complete();

            return new ApiResponse(201, "تم إرسال البلاغ بنجاح");
        }
    }
}