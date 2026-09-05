using Microsoft.EntityFrameworkCore;

namespace ZadElealm.Core.Specifications.Assessment;

public sealed class AssessmentDashboardSpecification : BaseSpecification<Models.Assessment>
{
    public AssessmentDashboardSpecification()
    {
        Includes.Add(x => x.Category);
        AddThenInclude(query => query
            .Include(x => x.Forms)
            .ThenInclude(form => form.Questions)
            .ThenInclude(question => question.Choices));
        AddOrderBy(x => x.Name);
        ApplySplitQuery();
    }
}
