using TeacherApp.App.Features.Login.ViewModels;

namespace TeacherApp.App.Features.Login.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
