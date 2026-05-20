using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Features.Login.Services;

namespace TeacherApp.App.Features.Login.ViewModels;

public partial class LoginViewModel(AuthService authService) : ObservableObject
{
    [ObservableProperty]
    private string _email = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string? _error;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            Error = "Preencha e-mail e senha.";
            return;
        }

        IsBusy = true;
        Error = null;

        try
        {
            await authService.LoginAsync(Email, Password);
            await Shell.Current.GoToAsync("//home");
        }
        catch (HttpRequestException)
        {
            Error = "Não foi possível conectar ao servidor.";
        }
        catch (Exception ex)
        {
            Error = ex.Message.Contains("401") || ex.Message.Contains("Unauthorized")
                ? "Credenciais inválidas."
                : "Erro ao fazer login.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
