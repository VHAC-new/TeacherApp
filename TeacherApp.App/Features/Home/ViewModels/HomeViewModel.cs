using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Core.Services;
using TeacherApp.App.Features.Home.Services;
using TeacherApp.App.Features.Login.Services;
using TeacherApp.Contracts.Modules;
using TeacherApp.Contracts.Progress;

namespace TeacherApp.App.Features.Home.ViewModels;

public partial class HomeViewModel(CatalogService catalog, ProgressService progress, AuthService auth) : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _error;

    public ObservableCollection<ModuleResponse> Modules { get; } = [];
    public ObservableCollection<ModuleProgressResponse> ModuleProgress { get; } = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        Error = null;

        try
        {
            var modules = await catalog.GetModulesAsync();
            Modules.Clear();
            foreach (var m in modules) Modules.Add(m);

            try
            {
                var overall = await progress.GetOverallAsync();
                ModuleProgress.Clear();
                foreach (var p in overall.Modules) ModuleProgress.Add(p);
            }
            catch
            {
                // Progress may fail if no data exists yet
            }
        }
        catch (HttpRequestException)
        {
            Error = "Não foi possível conectar ao servidor.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToModule(ModuleResponse module)
    {
        await Shell.Current.GoToAsync($"module?moduleId={module.Id}&title={Uri.EscapeDataString(module.Title)}");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        auth.Logout();
        await Shell.Current.GoToAsync("//login");
    }
}
