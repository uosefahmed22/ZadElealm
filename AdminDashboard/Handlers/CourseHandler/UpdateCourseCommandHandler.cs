using AdminDashboard.Commands.CourseCommand;
using AutoMapper;
using MediatR;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Service;

namespace AdminDashboard.Handlers.CourseHandler
{
    public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, ApiDataResponse>
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "AIzaSyD79T1TuP3_0uEFI7CftLd3bIzmd9PdelE";
        private const int MaxRetries = 3;
        private const int MaxResultsPerPage = 50;

        public UpdateCourseCommandHandler(IMediator mediator,
            IUnitOfWork unitOfWork, IMapper mapper, IImageService imageService, HttpClient httpClient)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
            _httpClient = httpClient;
        }
        public Task<ApiDataResponse> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
