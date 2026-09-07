using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models;

public class AssessmentProgress : BaseEntity
{
    public int Score { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? AttemptStartedAtUtc { get; set; }
    public DateTime? AttemptExpiresAtUtc { get; set; }
    public DateTime? AttemptSubmittedAtUtc { get; set; }
    public int AssessmentId { get; set; }
    public Assessment Assessment { get; set; } = null!;
    public int AssessmentFormId { get; set; }
    public AssessmentForm AssessmentForm { get; set; } = null!;
    public string AppUserId { get; set; } = string.Empty;
    public AppUser AppUser { get; set; } = null!;
}
