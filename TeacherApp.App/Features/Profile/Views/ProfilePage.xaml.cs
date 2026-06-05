using TeacherApp.App.Core;
using TeacherApp.App.Features.Profile.ViewModels;

namespace TeacherApp.App.Features.Profile.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _vm;

    public ProfilePage(ProfileViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is ICleanup cleanup)
            cleanup.Cleanup();

        base.OnDisappearing();
    }
}
