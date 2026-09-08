namespace ZadElealm.Core.Repositories;

public interface IRatingReadRepository
{
    Task<RatingSummaryReadModel> GetSummaryAsync(
        int courseId,
        CancellationToken cancellationToken = default);
}

public sealed record RatingSummaryReadModel(int Count, double Average);
