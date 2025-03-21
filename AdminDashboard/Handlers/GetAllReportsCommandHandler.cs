using AdminDashboard.Commands;
using AutoMapper;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Handlers;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;

namespace AdminDashboard.Handlers
{
    public class GetAllReportsCommandHandler : BaseCommandHandler<GetAllReportsCommand, ApiDataResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllReportsCommandHandler(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public override async Task<ApiDataResponse> Handle(GetAllReportsCommand request, CancellationToken cancellationToken)
        {
            var getAllReports = await _unitOfWork.Repository<Report>().GetAllAsync();

            var mappedReports = _mapper.Map<IEnumerable<Dto.ReportDto>>(getAllReports);
            return new ApiDataResponse(200, mappedReports);
        }
    }
}
