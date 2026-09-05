namespace ZadElealm.Core.Specifications.Assessment;

public sealed class AssessmentProgressSpecification : BaseSpecification<Models.AssessmentProgress>
{
    public AssessmentProgressSpecification(string userId, int assessmentId)
        : base(x => x.AppUserId == userId && x.AssessmentId == assessmentId)
    {
        Includes.Add(x => x.AppUser);
        Includes.Add(x => x.Assessment);
        Includes.Add(x => x.AssessmentForm);
    }

    public AssessmentProgressSpecification(string userId)
        : base(x => x.AppUserId == userId)
    {
    }
}
