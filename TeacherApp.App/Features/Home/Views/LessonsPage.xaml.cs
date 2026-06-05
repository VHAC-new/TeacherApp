using TeacherApp.App.Core;
using TeacherApp.App.Features.Home.ViewModels;

namespace TeacherApp.App.Features.Home.Views;

public partial class LessonsPage : ContentPage
{
    private readonly LessonsViewModel _vm;

    public LessonsPage(LessonsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is ICleanup cleanup)
            cleanup.Cleanup();

        base.OnDisappearing();
    }
}
