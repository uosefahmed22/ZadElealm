using AutoMapper;
using ZadElealm.Apis.Commands.Report;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Repositories;

namespace ZadElealm.Apis.Handlers.Report
{
    public class AddReportCommandHandler : BaseCommandHandler<AddReportCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AddReportCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public override async Task<ApiResponse> Handle(AddReportCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var mappedReport = _mapper.Map<Core.Models.Report>(request.ReportDto);
                mappedReport.UserId = request.UserId;

                await _unitOfWork.Repository<Core.Models.Report>().AddAsync(mappedReport);
                await _unitOfWork.Complete();

                return new ApiResponse(201, "تم إرسال البلاغ بنجاح");
            }
            catch (Exception)
            {
                return new ApiResponse(500, "حدث خطأ أثناء إرسال البلاغ");
            }
        }
    }
}
