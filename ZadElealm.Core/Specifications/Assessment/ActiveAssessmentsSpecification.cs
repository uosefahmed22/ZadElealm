using Microsoft.EntityFrameworkCore;

namespace ZadElealm.Core.Specifications.Assessment;

public sealed class ActiveAssessmentsSpecification : BaseSpecification<Models.Assessment>
{
    public ActiveAssessmentsSpecification()
        : base(x => x.IsActive)
    {
        Includes.Add(x => x.Category);
        AddOrderBy(x => x.Name);
    }
}
