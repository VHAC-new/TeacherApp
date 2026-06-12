using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Core;

namespace TeacherApp.App.Features.Home.ViewModels;

public partial class HomeViewModel : ObservableObject, ICleanup
{
    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string? _error;

    public int CurrentStreak => 7;

    [RelayCommand]
    private Task LoadAsync() => LoadInternalAsync(forceRefresh: false);

    [RelayCommand]
    private Task RefreshAsync() => LoadInternalAsync(forceRefresh: true);

    private Task LoadInternalAsync(bool forceRefresh)
    {
        Error = null;
        IsRefreshing = false;
        return Task.CompletedTask;
    }

    public void Cleanup()
    {
        IsRefreshing = false;
    }
}
