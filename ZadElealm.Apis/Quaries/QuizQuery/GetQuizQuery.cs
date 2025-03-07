using AutoMapper;
using ZadElealm.Apis.Dtos;
using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Handlers;
using ZadElealm.Core.Models;
using ZadElealm.Core.Repositories;
using ZadElealm.Core.Specifications;

namespace ZadElealm.Apis.Quaries.QuizQuery
{
    public class GetQuizQuery : BaseQuery<ApiResponse>
    {
        public int QuizId { get; }

        public GetQuizQuery(int quizId)
        {
            QuizId = quizId;
        }
    }
}
