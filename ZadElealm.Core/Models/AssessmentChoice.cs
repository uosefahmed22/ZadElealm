namespace ZadElealm.Core.Models;

public class AssessmentChoice : BaseEntity
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int AssessmentQuestionId { get; set; }
    public AssessmentQuestion AssessmentQuestion { get; set; } = null!;
}
