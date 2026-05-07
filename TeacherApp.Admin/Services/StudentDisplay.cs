using System.Globalization;
using MudBlazor;

namespace TeacherApp.Admin.Services;

public static class StudentDisplay
{
    public static string GetDisplayName(string? name, string email)
    {
        if (!string.IsNullOrWhiteSpace(name))
            return name;

        var local = email.Split('@')[0];
        var parts = local.Split(['.', '_', '-'], StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', parts.Select(p =>
            CultureInfo.InvariantCulture.TextInfo.ToTitleCase(p.ToLowerInvariant())));
    }

    public static string GetInitials(string? name, string email)
    {
        var display = GetDisplayName(name, email);
        var words = display.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
            return "?";

        if (words.Length == 1)
            return words[0][..1].ToUpperInvariant();

        return $"{char.ToUpperInvariant(words[0][0])}{char.ToUpperInvariant(words[^1][0])}";
    }

    public static Color GetAccuracyColor(double pct) => pct switch
    {
        >= 80 => Color.Success,
        >= 60 => Color.Warning,
        _ => Color.Error
    };
}
