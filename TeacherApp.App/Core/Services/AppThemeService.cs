namespace TeacherApp.App.Core.Services;

public sealed class AppThemeService
{
    public const string PreferenceKey = "app_theme";

    public void ApplySavedOrDefault(Application application)
    {
        var saved = Preferences.Default.Get(PreferenceKey, "light");
        application.UserAppTheme = saved == "dark" ? AppTheme.Dark : AppTheme.Light;
    }

    public bool IsDarkMode => Application.Current?.UserAppTheme == AppTheme.Dark;

    public string ToggleTitle => IsDarkMode ? "Light theme" : "Dark theme";

    public string ToggleSubtitle => IsDarkMode ? "Switch to white theme" : "Switch to dark theme";

    public string ToggleEmoji => IsDarkMode ? "☀️" : "🌙";

    public void Toggle()
    {
        if (Application.Current is not { } app)
            return;

        var dark = app.UserAppTheme != AppTheme.Dark;
        app.UserAppTheme = dark ? AppTheme.Dark : AppTheme.Light;
        Preferences.Default.Set(PreferenceKey, dark ? "dark" : "light");
    }
}
