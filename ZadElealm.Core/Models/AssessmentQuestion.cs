namespace ZadElealm.Core.Models;

public class AssessmentQuestion : BaseEntity
{
    public string Text { get; set; } = string.Empty;
    public int AssessmentFormId { get; set; }
    public AssessmentForm AssessmentForm { get; set; } = null!;
    public ICollection<AssessmentChoice> Choices { get; set; } = new List<AssessmentChoice>();
}
