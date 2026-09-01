using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications.Videos;

public sealed class CompletedVideoProgressSpecification : BaseSpecification<VideoProgress>
{
    public CompletedVideoProgressSpecification(string userId, int courseId)
        : base(x => x.UserId == userId && x.CourseId == courseId && x.IsCompleted)
    {
    }
}
