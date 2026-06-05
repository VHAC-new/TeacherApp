using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Features.Login.Services;

namespace TeacherApp.App.Features.Login.ViewModels;

public partial class LoginViewModel(AuthService authService) : ObservableObject
{
    private CancellationTokenSource? _cts;

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

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        IsBusy = true;
        Error = null;

        try
        {
            await authService.LoginAsync(Email, Password, ct);
            if (ct.IsCancellationRequested)
                return;

            await Shell.Current.GoToAsync("//home");
        }
        catch (OperationCanceledException)
        {
            // Ignorado se a operação foi cancelada.
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
            if (!ct.IsCancellationRequested)
                IsBusy = false;
        }
    }
}
