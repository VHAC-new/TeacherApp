using TeacherApp.App.ViewModels;

namespace TeacherApp.App.Views;

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
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
