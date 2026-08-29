using ZadElealm.Apis.Mappers;
using ZadElealm.Core.Errors;
using ZadElealm.Apis.Quaries.Certificate;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.ServiceDto;
using ZadElealm.Core.Specifications;
using ZadElealm.Core.Specifications.Certificate;

namespace ZadElealm.Apis.Handlers.CertificateHandler
{
    public class GetUserCertificatesQueryHandler : BaseQueryHandler<GetUserCertificatesQuery, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetUserCertificatesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<ApiResponse> Handle(GetUserCertificatesQuery request, CancellationToken cancellationToken)
        {
            var spec = new CertificateByUserAndQuizSpecification(request.UserId);
            var certificates = await _unitOfWork.Repository<Core.Models.Certificate>()
                .GetAllWithSpecNoTrackingAsync(spec);

            var certificateDtos = certificates.ToDtos();

            return new ApiDataResponse(200, certificateDtos);
        }
    }
}