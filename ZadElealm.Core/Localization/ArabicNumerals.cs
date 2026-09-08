using System.Globalization;
using System.Text;

namespace ZadElealm.Core.Localization;

public static class ArabicNumerals
{
    private static readonly char[] ArabicDigits = ['٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩'];
    private static readonly CultureInfo ArabicCulture = CultureInfo.GetCultureInfo("ar-EG");

    public static string Localize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        var result = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            result.Append(character is >= '0' and <= '9'
                ? ArabicDigits[character - '0']
                : character);
        }

        return result.ToString();
    }

    public static string Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value ?? string.Empty;

        var result = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            var digitIndex = Array.IndexOf(ArabicDigits, character);
            result.Append(digitIndex >= 0 ? (char)('0' + digitIndex) : character);
        }

        return result.ToString();
    }

    public static string Format(int value) => Localize(value.ToString("N0", ArabicCulture));

    public static string Format(double value, int maximumFractionDigits = 2)
    {
        var decimals = Math.Clamp(maximumFractionDigits, 0, 9);
        var format = decimals == 0 ? "0" : $"0.{new string('#', decimals)}";
        return Localize(value.ToString(format, ArabicCulture));
    }

    public static string FormatDate(DateTime value, string format = "d MMMM yyyy")
        => Localize(value.ToString(format, ArabicCulture));
}
