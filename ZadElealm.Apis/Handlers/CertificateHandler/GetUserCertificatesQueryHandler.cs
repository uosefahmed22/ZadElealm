using AutoMapper;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Quaries.Certificate;
using ZadElealm.Core.Models.ServiceDto;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Handlers.CertificateHandler
{
    public class GetUserCertificatesQueryHandler : BaseQueryHandler<GetUserCertificatesQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserCertificatesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public override async Task<ApiResponse> Handle(GetUserCertificatesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var spec = new CertificatesByUserSpecification(request.UserId);
                var certificates = await _unitOfWork.Repository<Core.Models.Certificate>()
                    .GetAllWithSpecAsync(spec);

                var certificateDtos = _mapper.Map<IReadOnlyList<CertificateDto>>(certificates);

                return new ApiDataResponse(200,certificateDtos);
            }
            catch (Exception ex)
            {
                return new ApiResponse(400, ex.Message);
            }
        }
    }
}
