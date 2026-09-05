using Microsoft.EntityFrameworkCore;

namespace ZadElealm.Core.Specifications.Assessment;

public sealed class AssessmentWithFormsSpecification : BaseSpecification<Models.Assessment>
{
    public AssessmentWithFormsSpecification(int assessmentId, bool activeOnly = true)
        : base(x => x.Id == assessmentId && (!activeOnly || x.IsActive))
    {
        Includes.Add(x => x.Category);
        AddThenInclude(query => query
            .Include(x => x.Forms)
            .ThenInclude(form => form.Questions)
            .ThenInclude(question => question.Choices));
        ApplySplitQuery();
    }
}
