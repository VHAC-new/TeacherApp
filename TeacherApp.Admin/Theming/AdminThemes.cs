using MudBlazor;

namespace TeacherApp.Admin.Theming;

public static class AdminThemes
{
    public static MudTheme Default { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#4F7CFF",
            Secondary = "#5c6bc0",
            Background = "#F8F9FB",
            Surface = "#FFFFFF",
            AppbarBackground = "#4F7CFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "rgba(0,0,0,.87)",
            LinesDefault = "#E5E7EB",
            Divider = "#E5E7EB",
            TextPrimary = "#1A1D29",
            TextSecondary = "#6B7280",
            Success = "#2e7d32",
            Warning = "#f57c00",
            Error = "#c62828",
            Info = "#0277bd"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#90caf9",
            Secondary = "#9fa8da",
            AppbarBackground = "#1e1e1e",
            Background = "#121212",
            Surface = "#1e1e1e",
            DrawerBackground = "#1e1e1e",
            DrawerText = "rgba(255,255,255,.87)",
            Success = "#81c784",
            Warning = "#ffb74d",
            Error = "#e57373",
            Info = "#4fc3f7"
        }
    };
}
