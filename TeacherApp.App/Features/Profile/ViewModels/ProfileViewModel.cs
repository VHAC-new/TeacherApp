using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using TeacherApp.App.Core;
using TeacherApp.App.Core.Messages;
using TeacherApp.App.Features.Home.Services;
using TeacherApp.App.Features.Profile.Models;

namespace TeacherApp.App.Features.Profile.ViewModels;

public partial class ProfileViewModel : ObservableObject, ICleanup
{
    private static readonly Color[] WorldColors =
    [
        Color.FromArgb("#7c5df7"),
        Color.FromArgb("#0ea5e9"),
        Color.FromArgb("#10b981"),
        Color.FromArgb("#f59e0b"),
    ];

    private readonly ProgressService _progress;

    private CancellationTokenSource? _cts;
    private bool _hasLoaded;

    public ProfileViewModel(ProgressService progress)
    {
        _progress = progress;

        // Mantém o perfil sincronizado quando uma aula é concluída em outra tela.
        // NÃO desregistrar no Cleanup: a mensagem chega enquanto o usuário está fora desta aba.
        WeakReferenceMessenger.Default.Register<ProgressChangedMessage>(
            this, static (r, _) => ((ProfileViewModel)r).OnProgressChanged());
    }

    [ObservableProperty]
    private string? _error;

    [ObservableProperty]
    private string _lessonsSummary = "";

    // Placeholders (a API ainda não fornece XP/streak — ver memória do projeto).
    public int Xp => 340;
    public int Streak => 7;

    public ObservableCollection<WeekDayProgress> Weekly { get; } =
    [
        new() { Label = "S" },
        new() { Label = "T" },
        new() { Label = "Q" },
        new() { Label = "Q" },
        new() { Label = "S" },
        new() { Label = "S", Studied = true },
        new() { Label = "D" },
    ];

    public ObservableCollection<WorldProgress> Worlds { get; } = [];

    [RelayCommand]
    private Task LoadAsync() => LoadInternalAsync(forceRefresh: false);

    // Recarrega o progresso quando uma aula é concluída (mensagem da tela de exercícios).
    private void OnProgressChanged() =>
        MainThread.BeginInvokeOnMainThread(() => _ = LoadInternalAsync(forceRefresh: true));

    private async Task LoadInternalAsync(bool forceRefresh)
    {
        if (!forceRefresh && _hasLoaded && Worlds.Count > 0)
            return;

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        Error = null;

        try
        {
            var overall = await _progress.GetOverallAsync(ct);
            if (ct.IsCancellationRequested) return;

            Worlds.Clear();
            int idx = 0;
            int totalCompleted = 0;
            int total = 0;

            foreach (var m in overall.Modules)
            {
                Worlds.Add(new WorldProgress
                {
                    Title = m.ModuleTitle,
                    Completed = m.CompletedLessons,
                    Total = m.TotalLessons,
                    Accent = WorldColors[idx % WorldColors.Length],
                });

                totalCompleted += m.CompletedLessons;
                total += m.TotalLessons;
                idx++;
            }

            LessonsSummary = $"{totalCompleted} de {total} lições concluídas";
            _hasLoaded = true;
        }
        catch (OperationCanceledException)
        {
            // Tela saiu ou carregamento substituído.
        }
        catch (HttpRequestException)
        {
            Error = "Não foi possível conectar ao servidor.";
        }
        catch (InvalidOperationException)
        {
            Error = "Resposta inesperada do servidor.";
        }
    }

    [RelayCommand]
    private async Task OpenSettings()
    {
        await Shell.Current.GoToAsync("settings");
    }

    public void Cleanup()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}
