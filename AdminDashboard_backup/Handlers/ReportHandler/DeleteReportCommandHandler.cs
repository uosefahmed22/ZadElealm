﻿using AdminDashboard.Commands.RoleCommand;
using MediatR;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Handlers.ReportHandler
{
    public class DeleteReportCommandHandler : IRequestHandler<DeleteReportCommand, ApiDataResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteReportCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiDataResponse> Handle(DeleteReportCommand request, CancellationToken cancellationToken)
        {
            var report = await _unitOfWork.Repository<Report>().GetEntityAsync(request.ReportId);

            if (report == null)
                return new ApiDataResponse(404, null, "التقرير غير موجود");

            _unitOfWork.Repository<Report>().Delete(report);
            await _unitOfWork.Complete();

            return new ApiDataResponse(200, null, "تم حذف التقرير بنجاح");
        }
    }
}
