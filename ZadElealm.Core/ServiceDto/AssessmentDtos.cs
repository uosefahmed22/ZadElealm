namespace ZadElealm.Core.ServiceDto;

public sealed class AssessmentSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int PassingScore { get; set; }
    public bool IsEligible { get; set; }
    public bool IsCompleted { get; set; }
    public int? BestScore { get; set; }
}

public sealed class AssessmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PassingScore { get; set; }
    public IReadOnlyList<AssessmentQuestionDto> Questions { get; set; } = [];
}

public sealed class AssessmentQuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public IReadOnlyList<AssessmentChoiceDto> Choices { get; set; } = [];
}

public sealed class AssessmentChoiceDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}

public sealed class AssessmentSubmissionDto
{
    public IReadOnlyList<StudentAnswerDto> StudentAnswers { get; set; } = [];
}

public sealed class AssessmentResultDto
{
    public string AssessmentName { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool IsCompleted { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public DateTime Date { get; set; }
}
