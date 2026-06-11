using System.Collections.ObjectModel;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Features.Login.Services;

namespace TeacherApp.App.Features.Login.ViewModels;

public partial class LoginViewModel(AuthService authService) : ObservableObject
{
    private const string RecentEmailsKey = "recent_login_emails";
    private const int MaxRecentEmails = 5;

    private CancellationTokenSource? _cts;
    private List<string> _allRecentEmails = [];

    [ObservableProperty]
    private string _email = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string? _error;

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<string> FilteredRecentEmails { get; } = [];

    public void LoadRecentEmails()
    {
        try
        {
            var json = Preferences.Get(RecentEmailsKey, "[]");
            _allRecentEmails = JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            _allRecentEmails = [];
        }
    }

    public void RefreshFilteredEmails()
    {
        FilteredRecentEmails.Clear();

        var query = Email?.Trim() ?? "";
        var matches = string.IsNullOrEmpty(query)
            ? _allRecentEmails
            : _allRecentEmails
                .Where(e => e.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

        foreach (var e in matches)
            FilteredRecentEmails.Add(e);
    }

    [RelayCommand]
    private void SelectRecentEmail(string email)
    {
        Email = email;
        FilteredRecentEmails.Clear();
    }

    private void SaveRecentEmail(string email)
    {
        var normalized = email.Trim().ToLowerInvariant();
        _allRecentEmails.Remove(normalized);
        _allRecentEmails.Insert(0, normalized);

        if (_allRecentEmails.Count > MaxRecentEmails)
            _allRecentEmails = _allRecentEmails.Take(MaxRecentEmails).ToList();

        try
        {
            Preferences.Set(RecentEmailsKey, JsonSerializer.Serialize(_allRecentEmails));
        }
        catch
        {
            // ignore storage failures
        }
    }

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

            SaveRecentEmail(Email);
            await Shell.Current.GoToAsync("//home");
        }
        catch (OperationCanceledException)
        {
            // ignored
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
