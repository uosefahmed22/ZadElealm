using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Handlers;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;
using ZadElealm.Core.Specifications;
using MediatR;
using AdminDashboard.Commands.ReportCommand;

namespace AdminDashboard.Handlers.ReportHandler
{
    public class HandleReportCommandHandler : IRequestHandler<HandleReportCommand, ApiDataResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISendEmailService _emailService;

        public HandleReportCommandHandler(IUnitOfWork unitOfWork, ISendEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        public async Task<ApiDataResponse> Handle(HandleReportCommand request, CancellationToken cancellationToken)
        {
            var spec = new ReportWithAppUserSpecification(request.ReportId);
            var report = await _unitOfWork.Repository<Report>().GetEntityWithSpecAsync(spec);

            if (report == null)
                return new ApiDataResponse(404, null, "التقرير غير موجود");

            report.AdminResponse = request.AdminResponse;
            report.IsSolved = true;
            _unitOfWork.Repository<Report>().Update(report);
            await _unitOfWork.Complete();

            var emailMessage = new EmailMessage
            {
                To = report.AppUser.Email,
                Subject = "تم معالجة تقريرك",
                Body = $@"
                <h2>تم معالجة تقريرك</h2>
                <p>عنوان التقرير: {report.TitleOfTheIssue}</p>
                <p>رد الإدارة: {request.AdminResponse}</p>"
            };

            await _emailService.SendEmailAsync(emailMessage, cancellationToken);

            return new ApiDataResponse(200, null, "تم معالجة التقرير بنجاح");
        }
    }
}
