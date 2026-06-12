using MudBlazor;

namespace TeacherApp.Admin.Theming;

public static class AdminThemes
{
    public static MudTheme Default { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#4F7CFF",
            Secondary = "#E8EEFF",
            Background = "#F8F9FB",
            Surface = "#FFFFFF",
            AppbarBackground = "#4F7CFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "rgba(0,0,0,.87)",
            LinesDefault = "#E5E7EB",
            Divider = "#E5E7EB",
            TextPrimary = "#1A1D29",
            TextSecondary = "#6B7280",
            Success = "#10B981",
            Warning = "#f57c00",
            Error = "#EF4444",
            Info = "#0277bd"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#4F7CFF",
            Secondary = "#1E2540",
            Background = "#0F1117",
            Surface = "#1A1E2E",
            AppbarBackground = "#1A1E2E",
            DrawerBackground = "#131724",
            DrawerText = "#E8ECF4",
            TextPrimary = "#E8ECF4",
            TextSecondary = "#8892A4",
            LinesDefault = "#2A3050",
            Divider = "#2A3050",
            Success = "#10B981",
            Warning = "#ffb74d",
            Error = "#EF4444",
            Info = "#4fc3f7"
        }
    };
}
