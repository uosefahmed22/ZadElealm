namespace ZadElealm.Core.Models;

public class AssessmentForm : BaseEntity
{
    // InternalCode is visible to administrators only. Student contracts never expose it.
    public string InternalCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int AssessmentId { get; set; }
    public Assessment Assessment { get; set; } = null!;
    public ICollection<AssessmentQuestion> Questions { get; set; } = new List<AssessmentQuestion>();
    public ICollection<AssessmentProgress> Progresses { get; set; } = new List<AssessmentProgress>();
}
