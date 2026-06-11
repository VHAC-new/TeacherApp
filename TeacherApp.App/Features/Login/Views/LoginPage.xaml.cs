using TeacherApp.App.Features.Login.ViewModels;

namespace TeacherApp.App.Features.Login.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _vm;

    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadRecentEmails();
    }

    private void OnEmailFocused(object? sender, FocusEventArgs e)
    {
        _vm.RefreshFilteredEmails();
        RecentEmailsContainer.IsVisible = _vm.FilteredRecentEmails.Count > 0;
    }

    private void OnEmailUnfocused(object? sender, FocusEventArgs e)
    {
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () =>
        {
            RecentEmailsContainer.IsVisible = false;
        });
    }

    private void OnEmailTextChanged(object? sender, TextChangedEventArgs e)
    {
        _vm.RefreshFilteredEmails();
        RecentEmailsContainer.IsVisible =
            EmailEntry.IsFocused && _vm.FilteredRecentEmails.Count > 0;
    }
}
