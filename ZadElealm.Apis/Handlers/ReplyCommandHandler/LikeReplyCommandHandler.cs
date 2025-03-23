using ZadElealm.Apis.Commands.ReplyCommand;
using ZadElealm.Apis.Errors;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications.ReplySpecifications;

namespace ZadElealm.Apis.Handlers.ReplyCommandHandler
{
    public class LikeReplyCommandHandler : BaseCommandHandler<LikeReplyCommand, ApiResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public LikeReplyCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public override async Task<ApiResponse> Handle(LikeReplyCommand request, CancellationToken cancellationToken)
        {
            var reply = await _unitOfWork.Repository<Reply>()
                .GetEntityAsync(request.ReplyId);
            if (reply == null)
            {
                return new ApiResponse(404, "الرد غير موجود");
            }

            var spec = new ReplyLikeWithLikesSpecification(request.ReplyId, request.AppUserId);
            var exsistingLike = await _unitOfWork.Repository<ReplyLike>()
                .GetEntityWithSpecAsync(spec);
            if (exsistingLike != null)
            {
                _unitOfWork.Repository<ReplyLike>().Delete(exsistingLike);
                await _unitOfWork.Complete();
                return new ApiResponse(200, "تم إلغاء الإعجاب");
            }
            else
            {
                var like = new ReplyLike
                {
                    ReplyId = request.ReplyId,
                    AppUserId = request.AppUserId
                };
                await _unitOfWork.Repository<ReplyLike>().AddAsync(like);
                await _unitOfWork.Complete();
                return new ApiResponse(200, "تم إضافة الإعجاب");
            }
        }
    }
}