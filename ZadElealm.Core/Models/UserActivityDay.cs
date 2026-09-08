using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Core.Models;

public sealed class UserActivityDay : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public DateOnly ActivityDate { get; set; }
    public AppUser User { get; set; } = null!;
}
