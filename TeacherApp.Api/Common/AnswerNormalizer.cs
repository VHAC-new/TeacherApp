using System.Text.RegularExpressions;

namespace TeacherApp.Api.Common;

public static partial class AnswerNormalizer
{
    public static string Normalize(string text, bool ignoreCase, bool ignoreWhitespace)
    {
        var result = text.Trim();

        if (ignoreWhitespace)
            result = CollapseWhitespace().Replace(result, " ");

        if (ignoreCase)
            result = result.ToLowerInvariant();

        return result;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex CollapseWhitespace();
}
