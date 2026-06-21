using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TeacherApp.App.Core;
using TeacherApp.App.Core.Services;
using TeacherApp.App.Features.Home.Models;
using TeacherApp.App.Features.Home.Services;
using TeacherApp.Contracts.Lessons;
using TeacherApp.Contracts.Progress;

namespace TeacherApp.App.Features.Home.ViewModels;

public partial class LessonsViewModel(CatalogService catalog, ProgressService progress) : ObservableObject, ICleanup
{
    // Emojis ilustrativos por lição (a API não fornece ícone por lição).
    private static readonly string[] LessonEmojis =
        ["👋", "💬", "🔢", "💡", "🎨", "👨‍👩‍👧", "⭐", "🍎", "🍽️", "🚌", "🛒", "☀️", "📘", "✏️", "🔤", "🗣️"];

    // Paleta por módulo (gradiente do cabeçalho + cor de destaque).
    private static readonly (string From, string To, string Accent, string Emoji)[] Palette =
    [
        ("#4f46e5", "#7c3aed", "#7c5df7", "🌊"),
        ("#0284c7", "#38bdf8", "#0ea5e9", "🏝️"),
        ("#059669", "#34d399", "#10b981", "🏞️"),
        ("#d97706", "#fbbf24", "#f59e0b", "🏔️"),
    ];

    private static readonly string[] Levels =
        ["Iniciante", "Básico", "Intermediário", "Avançado"];

    private CancellationTokenSource? _cts;
    private bool _hasLoaded;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string? _error;

    // Placeholders (a API ainda não fornece XP/streak — ver docs/qa).
    public int Xp => 340;
    public int Streak => 7;

    public ObservableCollection<TrailModule> Modules { get; } = [];

    [RelayCommand]
    private Task LoadAsync() => LoadInternalAsync(forceRefresh: false);

    [RelayCommand]
    private Task RefreshAsync() => LoadInternalAsync(forceRefresh: true);

    private async Task LoadInternalAsync(bool forceRefresh)
    {
        if (!forceRefresh && _hasLoaded && Modules.Count > 0)
        {
            IsRefreshing = false;
            return;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        Error = null;

        try
        {
            var modules = (await catalog.GetModulesAsync(ct))
                .OrderBy(m => m.Order)
                .ToList();
            if (ct.IsCancellationRequested) return;

            // Carrega lições + progresso de cada módulo em paralelo.
            var perModule = await Task.WhenAll(modules.Select(async m =>
            {
                var lessons = await catalog.GetLessonsAsync(m.Id, ct);
                var lessonProgress = await progress.GetLessonProgressAsync(m.Id, ct);
                return (Module: m, Lessons: lessons, Progress: lessonProgress);
            }));
            if (ct.IsCancellationRequested) return;

            Modules.Clear();

            // Um módulo é desbloqueado se o anterior foi 100% concluído.
            bool previousModuleCompleted = true;

            for (int mi = 0; mi < perModule.Length; mi++)
            {
                var (module, lessons, lessonProgress) = perModule[mi];
                bool moduleLocked = !previousModuleCompleted;

                var trail = BuildTrailModule(module, lessons, lessonProgress, mi, moduleLocked);
                Modules.Add(trail);

                previousModuleCompleted = trail.TotalCount > 0 && trail.CompletedCount == trail.TotalCount;
            }

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
        finally
        {
            IsRefreshing = false;
        }
    }

    private static TrailModule BuildTrailModule(
        TeacherApp.Contracts.Modules.ModuleResponse module,
        List<LessonResponse> lessons,
        List<LessonProgressResponse> lessonProgress,
        int moduleIndex,
        bool moduleLocked)
    {
        var palette = Palette[moduleIndex % Palette.Length];
        var level = Levels[System.Math.Min(moduleIndex, Levels.Length - 1)];
        var progressMap = lessonProgress.ToDictionary(p => p.LessonId);

        var ordered = lessons.OrderBy(l => l.Order).ToList();
        var nodes = new List<TrailNode>(ordered.Count);

        bool currentAssigned = false;
        bool previousCompleted = true;

        for (int i = 0; i < ordered.Count; i++)
        {
            var lesson = ordered[i];
            progressMap.TryGetValue(lesson.Id, out var lp);
            bool completed = lp?.IsCompleted ?? false;

            TrailNodeStatus status;
            if (moduleLocked)
            {
                status = TrailNodeStatus.Locked;
            }
            else if (completed)
            {
                status = TrailNodeStatus.Completed;
            }
            else if (previousCompleted && !currentAssigned)
            {
                status = TrailNodeStatus.Current;
                currentAssigned = true;
            }
            else
            {
                status = TrailNodeStatus.Locked;
            }

            nodes.Add(new TrailNode
            {
                Id = lesson.Id,
                ModuleId = module.Id,
                ModuleTitle = module.Title,
                Title = lesson.Title,
                Description = lesson.Description,
                AudioMediaId = lesson.AudioMediaId,
                Index = i,
                Status = status,
                Emoji = LessonEmojis[i % LessonEmojis.Length],
                IsBoss = i == ordered.Count - 1 && ordered.Count > 1,
            });

            previousCompleted = completed;
        }

        return new TrailModule
        {
            Id = module.Id,
            Title = module.Title,
            Subtitle = module.Description,
            Level = level,
            Order = module.Order,
            GradientFrom = palette.From,
            GradientTo = palette.To,
            AccentColor = palette.Accent,
            Emoji = palette.Emoji,
            Nodes = nodes,
            IsLocked = moduleLocked,
        };
    }

    [RelayCommand]
    private async Task NavigateToNode(TrailNode node)
    {
        if (node is null) return;

        if (node.IsLocked)
        {
            await Shell.Current.DisplayAlert(
                "Lição bloqueada",
                "Complete a lição anterior primeiro.",
                "OK");
            return;
        }

        var desc = Uri.EscapeDataString(node.Description ?? "");
        var audio = node.AudioMediaId is { } aid ? $"&audioMediaId={aid}" : "";
        var moduleTitle = Uri.EscapeDataString(node.ModuleTitle);
        await Shell.Current.GoToAsync(
            $"lesson?moduleId={node.ModuleId}&lessonId={node.Id}&title={Uri.EscapeDataString(node.Title)}&description={desc}&moduleTitle={moduleTitle}{audio}");
    }

    public void Cleanup()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        IsRefreshing = false;
    }
}
