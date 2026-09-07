namespace ZadElealm.Core.Policies;

public static class CourseCompletionPolicy
{
    public const int RequiredCompletionPercentage = 100;

    public static bool IsEligibleForAssessment(double completionPercentage)
        => completionPercentage >= RequiredCompletionPercentage;

    public static bool HasCompletedAllVideos(int completedVideos, int totalVideos)
        => totalVideos > 0 && completedVideos >= totalVideos;
}
