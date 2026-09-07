using Xunit;
using ZadElealm.Core.Policies;

namespace ZadElealm.UnitTests.Services;

public sealed class CourseCompletionPolicyTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(80, false)]
    [InlineData(99.99, false)]
    [InlineData(100, true)]
    public void AssessmentEligibility_RequiresOneHundredPercent(
        double completionPercentage,
        bool expected)
    {
        Assert.Equal(
            expected,
            CourseCompletionPolicy.IsEligibleForAssessment(completionPercentage));
    }

    [Theory]
    [InlineData(0, 0, false)]
    [InlineData(0, 1, false)]
    [InlineData(4, 5, false)]
    [InlineData(5, 5, true)]
    public void CompletedCourse_RequiresEveryVideo(
        int completedVideos,
        int totalVideos,
        bool expected)
    {
        Assert.Equal(
            expected,
            CourseCompletionPolicy.HasCompletedAllVideos(completedVideos, totalVideos));
    }
}
