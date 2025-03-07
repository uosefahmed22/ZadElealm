using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Handlers;
using ZadElealm.Core.Service;

namespace ZadElealm.Apis.Commands.Certificate
{
    public class GenerateCertificateCommand : BaseCommand<ApiResponse>
    {
        public int QuizId { get; }
        public string UserId { get; }

        public GenerateCertificateCommand(int quizId, string userId)
        {
            QuizId = quizId;
            UserId = userId;
        }
    }
}
