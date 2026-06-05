using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Core;
using TeacherApp.App.Core.Services;
using TeacherApp.App.Features.Login.Services;

namespace TeacherApp.App.Features.Profile.ViewModels;

public partial class ProfileViewModel(AuthService auth, TokenStore tokenStore) : ObservableObject, ICleanup
{
    [ObservableProperty]
    private string _displayName = "Student";

    [ObservableProperty]
    private string _email = "";

    [RelayCommand]
    private void Load()
    {
        Email = tokenStore.Email ?? "";
        DisplayName = BuildDisplayName(tokenStore.Email);
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

    public void Cleanup()
    {
    }
}
