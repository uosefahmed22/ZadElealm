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
    public class GetAllReportsCommandHandler : BaseCommandHandler<GetAllReportsCommand, ApiDataResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllReportsCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiDataResponse> Handle(GetAllReportsCommand request, CancellationToken cancellationToken)
        {
            var getAllReports = await _unitOfWork.Repository<Report>().GetAllAsync();
            var mappedReports = getAllReports.ToDtos();
            return new ApiDataResponse(200, mappedReports);
        }
    }
}
