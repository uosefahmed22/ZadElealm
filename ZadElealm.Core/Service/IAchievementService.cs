using ZadElealm.Core.ServiceDto;

namespace ZadElealm.Core.Service;

public interface IAchievementService
{
    Task<AchievementDashboardDto> GetMineAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<AchievementDashboardDto> CheckInAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
