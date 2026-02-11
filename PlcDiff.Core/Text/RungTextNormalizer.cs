using System.Text.RegularExpressions;

namespace PlcDiff.Core.Text;

public static class RungTextNormalizer
{
    private static readonly Regex SpaceCollapseRegex = new("[ \t]+", RegexOptions.Compiled);

    public static string Normalize(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return string.Empty;
        }

        var normalized = rawText.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Trim();

        normalized = SpaceCollapseRegex.Replace(normalized, " ");

        return normalized;
    }
}
