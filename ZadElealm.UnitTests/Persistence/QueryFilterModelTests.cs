using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.UnitTests.Persistence;

public class QueryFilterModelTests
{
    [Fact]
    public void Model_HasNoRequiredNavigationQueryFilterWarnings()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"query-filter-model-{Guid.NewGuid()}")
            .EnableServiceProviderCaching(false)
            .ConfigureWarnings(warnings => warnings.Throw(
                CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning))
            .Options;

        using var context = new AppDbContext(options);

        _ = context.Model;
    }
}
