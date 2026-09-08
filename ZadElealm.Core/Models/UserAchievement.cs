using ZadElealm.Core.Enums;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models;

public sealed class UserAchievement : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public AchievementCode Code { get; set; }
    public DateTime UnlockedAtUtc { get; set; }
    public AppUser User { get; set; } = null!;
}
