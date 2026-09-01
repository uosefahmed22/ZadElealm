using ZadElealm.Apis.Commands.Report;
using ZadElealm.Core.Errors;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Enums;

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
            if (!Enum.TryParse<ReportType>(request.ReportDto.ReportType, out var reportType))
            {
                return new ApiResponse(400, "نوع البلاغ غير صالح");
            }

            var mappedReport = new Core.Models.Report
            {
                AppUserId = request.UserId,
                TitleOfTheIssue = request.ReportDto.TitleOfTheIssue.Trim(),
                Description = request.ReportDto.Description.Trim(),
                reportTypes = reportType,
                AdminResponse = null,
                IsSolved = false
            };

            await _unitOfWork.Repository<Core.Models.Report>().AddAsync(mappedReport);
            await _unitOfWork.Complete();

            return new ApiResponse(201, "تم إرسال البلاغ بنجاح");
        }
    }
}
