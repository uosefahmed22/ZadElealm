using ZadElealm.Core.Localization;

namespace ZadElealm.UnitTests.Localization;

public sealed class ArabicNumeralsTests
{
    [Theory]
    [InlineData("1234567890", "١٢٣٤٥٦٧٨٩٠")]
    [InlineData("12:05", "١٢:٠٥")]
    public void Localize_ReplacesAsciiDigits(string input, string expected)
        => Assert.Equal(expected, ArabicNumerals.Localize(input));

    [Fact]
    public void Normalize_AcceptsArabicAndAsciiDigits()
    {
        Assert.Equal("123456", ArabicNumerals.Normalize("١٢٣456"));
    }

    [Fact]
    public void FormatDate_UsesArabicMonthAndDigits()
    {
        var result = ArabicNumerals.FormatDate(new DateTime(2026, 8, 30));

        Assert.Contains("٣٠", result);
        Assert.Contains("أغسطس", result);
        Assert.Contains("٢٠٢٦", result);
    }
}
