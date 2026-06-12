using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Core;
using TeacherApp.App.Features.Login.Services;

namespace TeacherApp.App.Features.Profile.ViewModels;

public partial class ChangePasswordViewModel(AuthService auth) : ObservableObject, ICleanup
{
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private string _currentPassword = "";

    [ObservableProperty]
    private string _newPassword = "";

    [ObservableProperty]
    private string _confirmPassword = "";

    [ObservableProperty]
    private string? _error;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isSuccess;

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (IsBusy)
            return;

        Error = null;
        IsSuccess = false;

        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            Error = "All fields are required.";
            return;
        }

        if (NewPassword.Length < 6)
        {
            Error = "New password must be at least 6 characters.";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            Error = "Passwords do not match.";
            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        IsBusy = true;
        try
        {
            await auth.ChangePasswordAsync(CurrentPassword, NewPassword, ct);
            if (ct.IsCancellationRequested)
                return;

            IsSuccess = true;
            CurrentPassword = "";
            NewPassword = "";
            ConfirmPassword = "";
        }
        catch (OperationCanceledException)
        {
            // Screen left or request superseded.
        }
        catch (HttpRequestException)
        {
            Error = "Could not change password. Check your current password.";
        }
        finally
        {
            if (!ct.IsCancellationRequested)
                IsBusy = false;
        }
    }

    public void Cleanup()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        IsBusy = false;
    }
}
