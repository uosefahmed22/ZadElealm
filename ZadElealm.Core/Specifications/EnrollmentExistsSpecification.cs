using ZadElealm.Core.Models;

namespace ZadElealm.Core.Specifications;

public sealed class EnrollmentExistsSpecification : BaseSpecification<Enrollment>
{
    public EnrollmentExistsSpecification(int courseId, string userId)
        : base(x => x.CourseId == courseId && x.AppUserId == userId)
    {
    }
}
