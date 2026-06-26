using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Core;
using TeacherApp.App.Core.Services;
using TeacherApp.App.Features.Home.Services;
using TeacherApp.App.Features.Module.Models;

namespace TeacherApp.App.Features.Module.ViewModels;

[QueryProperty(nameof(ModuleId), "moduleId")]
[QueryProperty(nameof(Title), "title")]
public partial class ModuleViewModel(CatalogService catalog, ProgressService progress) : ObservableObject, ICleanup
{
    private CancellationTokenSource? _cts;
    private string? _loadedModuleId;
    private Guid? _lastTrailId;

    [ObservableProperty]
    private string _moduleId = "";

    [ObservableProperty]
    private string _title = "";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string? _error;

    [ObservableProperty]
    private int _completedLessons;

    [ObservableProperty]
    private int _totalLessons;

    [ObservableProperty]
    private double _progressPercent;

    [ObservableProperty]
    private string _progressText = "";

    public ObservableCollection<LessonWithProgress> Lessons { get; } = [];

    [RelayCommand]
    private Task LoadAsync() => LoadInternalAsync(forceRefresh: false);

    [RelayCommand]
    private Task RefreshAsync() => LoadInternalAsync(forceRefresh: true);

    private async Task LoadInternalAsync(bool forceRefresh)
    {
        if (!Guid.TryParse(ModuleId, out var id))
        {
            IsRefreshing = false;
            return;
        }

        if (!forceRefresh && Lessons.Count > 0 && _loadedModuleId == ModuleId)
        {
            IsRefreshing = false;
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
            // Módulo › Trilhas › Aulas: agrega as aulas de todas as trilhas do módulo,
            // mantendo a trava sequencial (trilha por trilha) na visão geral.
            var trails = (await catalog.GetTrailsAsync(id, ct)).OrderBy(t => t.Order).ToList();
            if (ct.IsCancellationRequested) return;

            Lessons.Clear();
            bool previousCompleted = true;
            foreach (var trail in trails)
            {
                var lessons = await catalog.GetLessonsAsync(trail.Id, ct);
                var lessonProgress = await progress.GetLessonProgressAsync(trail.Id, ct);
                if (ct.IsCancellationRequested) return;

                var progressMap = lessonProgress.ToDictionary(p => p.LessonId);
                foreach (var lesson in lessons.OrderBy(l => l.Order))
                {
                    progressMap.TryGetValue(lesson.Id, out var lp);
                    bool isLocked = !previousCompleted;
                    Lessons.Add(new LessonWithProgress(lesson, lp, isLocked));
                    previousCompleted = lp?.IsCompleted ?? false;
                }
            }

            _lastTrailId = trails.Count > 0 ? trails[^1].Id : null;

            TotalLessons = Lessons.Count;
            CompletedLessons = Lessons.Count(l => l.IsCompleted);
            ProgressPercent = TotalLessons > 0 ? (double)CompletedLessons / TotalLessons : 0;
            ProgressText = $"{CompletedLessons} of {TotalLessons} lessons completed";
            _loadedModuleId = ModuleId;
        }
        catch (OperationCanceledException) { }
        catch (HttpRequestException)
        {
            Error = "Erro ao carregar lições.";
        }
        finally
        {
            IsRefreshing = false;
            if (!ct.IsCancellationRequested)
                IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToLesson(LessonWithProgress lesson)
    {
        if (lesson.IsLocked)
        {
            await Shell.Current.DisplayAlert(
                "Lição bloqueada",
                "Complete a lição anterior primeiro.",
                "OK");
            return;
        }

        var desc = Uri.EscapeDataString(lesson.Description ?? "");
        var audio = lesson.AudioMediaId is { } aid ? $"&audioMediaId={aid}" : "";
        var moduleTitle = Uri.EscapeDataString(Title);
        await Shell.Current.GoToAsync(
            $"lesson?moduleId={ModuleId}&trailId={lesson.TrailId}&lessonId={lesson.Id}&title={Uri.EscapeDataString(lesson.Title)}&description={desc}&moduleTitle={moduleTitle}{audio}");
    }

    [RelayCommand]
    private async Task NavigateToFinalExercises()
    {
        // Exercícios finais agora pertencem à trilha; usa a última trilha do módulo (prova final).
        if (_lastTrailId is not { } trailId)
            return;

        await Shell.Current.GoToAsync(
            $"final-exercises?trailId={trailId}&title={Uri.EscapeDataString(Title)}");
    }

    public void Cleanup()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _loadedModuleId = null;
        IsBusy = false;
        IsRefreshing = false;
    }
}
