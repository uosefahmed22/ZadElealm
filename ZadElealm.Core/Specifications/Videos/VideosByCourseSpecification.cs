using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications.Videos;

public sealed class VideosByCourseSpecification : BaseSpecification<Video>
{
    public VideosByCourseSpecification(int courseId)
        : base(x => x.CourseId == courseId)
    {
    }
}
