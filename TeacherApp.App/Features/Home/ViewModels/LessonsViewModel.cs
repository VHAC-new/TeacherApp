using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Core;
using TeacherApp.App.Core.Services;
using TeacherApp.Contracts.Modules;

namespace TeacherApp.App.Features.Home.ViewModels;

public partial class LessonsViewModel(CatalogService catalog) : ObservableObject, ICleanup
{
    private CancellationTokenSource? _cts;
    private bool _hasLoaded;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _error;

    public ObservableCollection<ModuleResponse> Modules { get; } = [];

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (_hasLoaded && Modules.Count > 0)
            return;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        IsBusy = true;
        Error = null;

        try
        {
            var modules = await catalog.GetModulesAsync(ct);
            if (ct.IsCancellationRequested)
                return;

            Modules.Clear();
            foreach (var m in modules)
                Modules.Add(m);

            _hasLoaded = true;
        }
        catch (OperationCanceledException)
        {
            // Tela saiu ou carregamento substituído.
        }
        catch (HttpRequestException)
        {
            Error = "Não foi possível conectar ao servidor.";
        }
        finally
        {
            if (!ct.IsCancellationRequested)
                IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToModule(ModuleResponse module)
    {
        await Shell.Current.GoToAsync($"module?moduleId={module.Id}&title={Uri.EscapeDataString(module.Title)}");
    }

    public void Cleanup()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        IsBusy = false;
    }
}
