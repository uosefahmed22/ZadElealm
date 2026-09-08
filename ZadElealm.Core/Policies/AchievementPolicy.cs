using ZadElealm.Core.Enums;

namespace ZadElealm.Core.Policies;

public static class AchievementPolicy
{
    public static IReadOnlyList<AchievementDefinition> Definitions { get; } =
    [
        new(AchievementCode.FirstLesson, "البداية", "أكمل أول درس في رحلتك التعليمية.", "first-lesson", 1, AchievementMetric.CompletedLessons),
        new(AchievementCode.FirstCourse, "أول إنجاز", "أكمل أول دورة واجتز اختبارها.", "first-course", 1, AchievementMetric.CompletedCourses),
        new(AchievementCode.FiveCourses, "ثابت الخطى", "أكمل خمس دورات تعليمية.", "five-courses", 5, AchievementMetric.CompletedCourses),
        new(AchievementCode.TenCourses, "طالب علم", "أكمل عشر دورات تعليمية.", "ten-courses", 10, AchievementMetric.CompletedCourses),
        new(AchievementCode.SevenDayStreak, "أسبوع من الهمة", "زر المنصة سبعة أيام متتالية.", "seven-day-streak", 7, AchievementMetric.LongestStreak),
        new(AchievementCode.ThirtyDayStreak, "شهر من الثبات", "زر المنصة ثلاثين يومًا متتاليًا.", "thirty-day-streak", 30, AchievementMetric.LongestStreak),
        new(AchievementCode.QuizExcellence, "متفوّق", "احصل على 90% أو أكثر في ثلاثة اختبارات.", "quiz-excellence", 3, AchievementMetric.HighScoreQuizzes),
        new(AchievementCode.SilverRank, "المستوى الفضي", "اجمع 100 نقطة للوصول إلى المستوى الفضي.", "silver-rank", 100, AchievementMetric.TotalPoints),
        new(AchievementCode.GoldRank, "المستوى الذهبي", "اجمع 300 نقطة للوصول إلى المستوى الذهبي.", "gold-rank", 300, AchievementMetric.TotalPoints),
        new(AchievementCode.PlatinumRank, "المستوى البلاتيني", "اجمع 600 نقطة للوصول إلى المستوى البلاتيني.", "platinum-rank", 600, AchievementMetric.TotalPoints),
        new(AchievementCode.DiamondRank, "القمة", "اجمع 1000 نقطة للوصول إلى المستوى الماسي.", "diamond-rank", 1000, AchievementMetric.TotalPoints)
    ];

    public static int CurrentValue(AchievementDefinition definition, AchievementMetrics metrics)
        => definition.Metric switch
        {
            AchievementMetric.CompletedLessons => metrics.CompletedLessons,
            AchievementMetric.CompletedCourses => metrics.CompletedCourses,
            AchievementMetric.LongestStreak => metrics.LongestStreak,
            AchievementMetric.HighScoreQuizzes => metrics.HighScoreQuizzes,
            AchievementMetric.TotalPoints => metrics.TotalPoints,
            _ => 0
        };

    public static IReadOnlyList<AchievementCode> EligibleAchievements(AchievementMetrics metrics)
        => Definitions
            .Where(definition => CurrentValue(definition, metrics) >= definition.Target)
            .Select(definition => definition.Code)
            .ToList();

    public static StreakSummary CalculateStreaks(
        IEnumerable<DateOnly> activityDates,
        DateOnly today)
    {
        var dates = activityDates
            .Where(date => date <= today)
            .Distinct()
            .OrderBy(date => date)
            .ToList();
        if (dates.Count == 0)
            return new StreakSummary(0, 0);

        var longest = 1;
        var running = 1;
        for (var index = 1; index < dates.Count; index++)
        {
            running = dates[index].DayNumber == dates[index - 1].DayNumber + 1
                ? running + 1
                : 1;
            longest = Math.Max(longest, running);
        }

        var latest = dates[^1];
        if (latest < today.AddDays(-1))
            return new StreakSummary(0, longest);

        var current = 0;
        var expected = latest;
        for (var index = dates.Count - 1; index >= 0; index--)
        {
            if (dates[index] > expected)
                continue;
            if (dates[index] != expected)
                break;

            current++;
            expected = expected.AddDays(-1);
        }

        return new StreakSummary(current, longest);
    }
}

public enum AchievementMetric
{
    CompletedLessons,
    CompletedCourses,
    LongestStreak,
    HighScoreQuizzes,
    TotalPoints
}

public sealed record AchievementDefinition(
    AchievementCode Code,
    string Title,
    string Description,
    string IconKey,
    int Target,
    AchievementMetric Metric);

public sealed record AchievementMetrics(
    int CompletedLessons,
    int CompletedCourses,
    int HighScoreQuizzes,
    int TotalPoints,
    int CurrentStreak,
    int LongestStreak);

public sealed record StreakSummary(int Current, int Longest);
