namespace AdminDashboard.Models;

public sealed class AssessmentDashboardIndexViewModel
{
    public IReadOnlyList<AssessmentDashboardItemViewModel> Assessments { get; init; } = [];
}

public sealed class AssessmentDashboardItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public int PassingScore { get; init; }
    public bool IsActive { get; init; }
    public int FormCount { get; init; }
    public int QuestionCount { get; init; }
}

public sealed class AssessmentDashboardDetailsViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public int PassingScore { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<AssessmentFormAdminViewModel> Forms { get; init; } = [];
}

public sealed class AssessmentFormAdminViewModel
{
    public int Id { get; init; }
    public string InternalCode { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public IReadOnlyList<AssessmentQuestionAdminViewModel> Questions { get; init; } = [];
}

public sealed class AssessmentQuestionAdminViewModel
{
    public int Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public IReadOnlyList<AssessmentChoiceAdminViewModel> Choices { get; init; } = [];
}

public sealed class AssessmentChoiceAdminViewModel
{
    public string Text { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
}
