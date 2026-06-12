using TeacherApp.App.Core;
using TeacherApp.App.Features.Module.ViewModels;

namespace TeacherApp.App.Features.Module.Views;

public partial class ModulePage : ContentPage
{
    private readonly ModuleViewModel _vm;

    public ModulePage(ModuleViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.RefreshCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is ICleanup cleanup)
            cleanup.Cleanup();
        base.OnDisappearing();
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
