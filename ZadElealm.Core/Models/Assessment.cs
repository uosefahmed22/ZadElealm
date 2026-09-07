namespace ZadElealm.Core.Models;

public class Assessment : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PassingScore { get; set; } = 60;
    public int DurationMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<AssessmentForm> Forms { get; set; } = new List<AssessmentForm>();
    public ICollection<AssessmentProgress> Progresses { get; set; } = new List<AssessmentProgress>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}
