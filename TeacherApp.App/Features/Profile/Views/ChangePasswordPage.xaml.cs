using TeacherApp.App.Core;
using TeacherApp.App.Features.Profile.ViewModels;

namespace TeacherApp.App.Features.Profile.Views;

public partial class ChangePasswordPage : ContentPage
{
    public ChangePasswordPage(ChangePasswordViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
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
