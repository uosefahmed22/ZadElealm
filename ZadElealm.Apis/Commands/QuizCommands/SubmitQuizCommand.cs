﻿using ZadElealm.Apis.Errors;
using ZadElealm.Apis.Handlers;
using ZadElealm.Core.Service;
using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Apis.Commands.QuizCommands
{
    public class SubmitQuizCommand : BaseCommand<ApiResponse>
    {
        public string UserId { get; }
        public QuizSubmissionDto Submission { get; }

        public SubmitQuizCommand(string userId, QuizSubmissionDto submission)
        {
            UserId = userId;
            Submission = submission;
        }
    }
}
