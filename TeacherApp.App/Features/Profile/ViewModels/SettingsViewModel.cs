using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Core;
using TeacherApp.App.Core.Services;
using TeacherApp.App.Features.Login.Services;

namespace TeacherApp.App.Features.Profile.ViewModels;

public partial class SettingsViewModel(
    AuthService auth,
    TokenStore tokenStore,
    AppThemeService themeService) : ObservableObject, ICleanup
{
    [ObservableProperty]
    private string _displayName = "Student";

    [ObservableProperty]
    private string _email = "";

    [ObservableProperty]
    private string _initials = "S";

    [ObservableProperty]
    private string _themeTitle = "";

    [ObservableProperty]
    private string _themeSubtitle = "";

    [ObservableProperty]
    private string _themeEmoji = "";

    [RelayCommand]
    private void Load()
    {
        Email = tokenStore.Email ?? "";
        DisplayName = BuildDisplayName(tokenStore.Email);
        Initials = BuildInitials(DisplayName);
        RefreshThemeLabels();
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        themeService.Toggle();
        RefreshThemeLabels();
    }

    private void RefreshThemeLabels()
    {
        ThemeTitle = themeService.ToggleTitle;
        ThemeSubtitle = themeService.ToggleSubtitle;
        ThemeEmoji = themeService.ToggleEmoji;
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        await Shell.Current.GoToAsync("change-password");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        auth.Logout();
        await Shell.Current.GoToAsync("//login");
    }

    private static string BuildDisplayName(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "Student";

        var local = email.Split('@')[0];
        if (string.IsNullOrWhiteSpace(local))
            return "Student";

        return char.ToUpperInvariant(local[0]) + local[1..];
    }

    private static string BuildInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "S";

        var parts = name.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[1][0])}";

        return char.ToUpperInvariant(name[0]).ToString();
    }

    public void Cleanup()
    {
    }
}
