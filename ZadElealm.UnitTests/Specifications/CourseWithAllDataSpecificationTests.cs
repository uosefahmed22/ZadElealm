using Xunit;
using ZadElealm.Core.Specifications.Course;

namespace ZadElealm.UnitTests.Specifications;

public class CourseWithAllDataSpecificationTests
{
    [Fact]
    public void Constructor_EnablesSplitQueryForMultipleCollections()
    {
        var specification = new CourseWithAllDataSpecification(162);

        Assert.True(specification.IsSplitQuery);
    }
}
