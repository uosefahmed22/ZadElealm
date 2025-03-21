using AdminDashboard.Commands;
using AutoMapper;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Handlers;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Handlers
{
    public class GetReportDetailsCommandHandler : BaseCommandHandler<GetReportDetailsCommand, ApiDataResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetReportDetailsCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public override async Task<ApiDataResponse> Handle(GetReportDetailsCommand request, CancellationToken cancellationToken)
        {
            var report = await _unitOfWork.Repository<Report>().GetEntityWithNoTrackingAsync(request.Id);
            if (report == null)
                return new ApiDataResponse(404, null, "التقرير غير موجود");

            var mappedReport = _mapper.Map<Dto.ReportDto>(report);
            return new ApiDataResponse(200, mappedReport);
        }
    }
}
