using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.ApplicationModel;
using TeacherApp.App.Core;
using TeacherApp.App.Core.Messages;
using TeacherApp.App.Core.Services;
using TeacherApp.App.Features.Home.Models;
using TeacherApp.App.Features.Home.Services;
using TeacherApp.Contracts.Lessons;
using TeacherApp.Contracts.Modules;
using TeacherApp.Contracts.Progress;

namespace TeacherApp.App.Features.Home.ViewModels;

public enum LessonsPageState { Loading, Ready, Error }

public partial class LessonsViewModel : ObservableObject, ICleanup
{
    // Emojis ilustrativos por lição (a API não fornece ícone por lição).
    private static readonly string[] LessonEmojis =
        ["👋", "💬", "🔢", "💡", "🎨", "👨‍👩‍👧", "⭐", "🍎", "🍽️", "🚌", "🛒", "☀️", "📘", "✏️", "🔤", "🗣️"];

    // Paleta por módulo (gradiente do cabeçalho + cor de destaque).
    private static readonly (string From, string To, string Accent, string Emoji)[] Palette =
    [
        ("#e11d48", "#fb7185", "#f43f5e", "🌊"),
        ("#0284c7", "#38bdf8", "#0ea5e9", "🏝️"),
        ("#059669", "#34d399", "#10b981", "🏞️"),
        ("#d97706", "#fbbf24", "#f59e0b", "🏔️"),
    ];

    private static readonly string[] Levels =
        ["Iniciante", "Básico", "Intermediário", "Avançado"];

    // Janela em que uma troca de aba reutiliza a memória sem rebater na API.
    private static readonly TimeSpan TabSwitchTtl = TimeSpan.FromSeconds(45);

    private readonly CatalogService _catalog;
    private readonly ProgressService _progress;
    private readonly AppSessionState _appSession;

    private CancellationTokenSource? _cts;
    private bool _hasLoaded;
    private DateTimeOffset _lastSyncUtc;

    public LessonsViewModel(CatalogService catalog, ProgressService progress, AppSessionState appSession)
    {
        _catalog = catalog;
        _progress = progress;
        _appSession = appSession;

        // Mantém a trilha sincronizada quando uma aula é concluída em outra tela.
        // NÃO desregistrar no Cleanup: a mensagem chega enquanto o usuário está fora desta aba.
        WeakReferenceMessenger.Default.Register<ProgressChangedMessage>(
            this, static (r, m) => ((LessonsViewModel)r).OnProgressChanged(m));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading))]
    [NotifyPropertyChangedFor(nameof(IsReady))]
    [NotifyPropertyChangedFor(nameof(IsError))]
    private LessonsPageState _state = LessonsPageState.Loading;

    [ObservableProperty]
    private string? _error;

    public bool IsLoading => State == LessonsPageState.Loading;
    public bool IsReady => State == LessonsPageState.Ready;
    public bool IsError => State == LessonsPageState.Error;

    // Placeholders (a API ainda não fornece XP/streak — ver docs/qa).
    public int Xp => 340;
    public int Streak => 7;

    public ObservableCollection<TrailModule> Modules { get; } = [];

    private CancellationToken ResetCts()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        return _cts.Token;
    }

    /// <summary>Chamado em OnAppearing. Primeira vez: carrega (fase 1 + 1º módulo). Demais: refresh silencioso.</summary>
    [RelayCommand]
    private async Task InitializeAsync()
    {
        if (_hasLoaded && Modules.Count > 0)
        {
            // Troca de aba: reusa a memória. Só revalida (silencioso) se o app retomou do
            // background (resume) ou se passou o TTL desde a última sincronização.
            bool resume = _appSession.ConsumeResyncRequest();
            bool stale = DateTimeOffset.UtcNow - _lastSyncUtc >= TabSwitchTtl;
            if (resume || stale)
                await RefreshProgressAsync();
            return;
        }

        await LoadFirstTimeAsync();
    }

    [RelayCommand]
    private Task RetryAsync()
    {
        _hasLoaded = false;
        return LoadFirstTimeAsync();
    }

    // ─── Fase 1: shells (barato) ─────────────────────────────────────────────
    private async Task LoadFirstTimeAsync()
    {
        var ct = ResetCts();
        State = LessonsPageState.Loading;
        Error = null;

        try
        {
            var modules = (await _catalog.GetModulesAsync(ct)).OrderBy(m => m.Order).ToList();
            var overall = await _progress.GetOverallAsync(ct);
            if (ct.IsCancellationRequested) return;

            var progressByModule = overall.Modules.ToDictionary(m => m.ModuleId);

            Modules.Clear();
            bool previousCompleted = true;

            for (int mi = 0; mi < modules.Count; mi++)
            {
                var module = modules[mi];
                progressByModule.TryGetValue(module.Id, out var mp);
                int total = mp?.TotalLessons ?? module.LessonCount;
                int completed = mp?.CompletedLessons ?? 0;
                bool locked = !previousCompleted;

                Modules.Add(BuildShell(module, mi, locked, total, completed));

                previousCompleted = total > 0 && completed == total;
            }

            // Carrega os nós do primeiro módulo antes de exibir a trilha (evita trilha vazia).
            if (Modules.Count > 0)
                await EnsureModuleLoadedAsync(Modules[0], ct);
            if (ct.IsCancellationRequested) return;

            _hasLoaded = true;
            _lastSyncUtc = DateTimeOffset.UtcNow;
            _appSession.ConsumeResyncRequest(); // limpa flag de launch para não revalidar logo em seguida
            State = LessonsPageState.Ready;
        }
        catch (OperationCanceledException)
        {
            // Tela saiu ou carregamento substituído.
        }
        catch (HttpRequestException)
        {
            Error = "Não foi possível conectar ao servidor.";
            State = LessonsPageState.Error;
        }
    }

    private static TrailModule BuildShell(ModuleResponse module, int moduleIndex, bool locked, int total, int completed)
    {
        var palette = Palette[moduleIndex % Palette.Length];
        var level = Levels[Math.Min(moduleIndex, Levels.Length - 1)];

        var shell = new TrailModule
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
            IsLocked = locked,
            TotalCount = total,
            CompletedCount = completed,
            LoadState = TrailModuleLoadState.NotLoaded,
        };

        // Módulo bloqueado: a trilha inteira é exibida com nós bloqueados. Como esses nós não
        // têm título/navegação, sintetizamos a partir da contagem conhecida — sem requisição.
        if (locked)
        {
            shell.Nodes = BuildLockedNodes(module.Id, module.Title, total);
            shell.LoadState = TrailModuleLoadState.Loaded;
        }

        return shell;
    }

    private static List<TrailNode> BuildLockedNodes(Guid moduleId, string moduleTitle, int count)
    {
        var nodes = new List<TrailNode>(System.Math.Max(0, count));
        for (int i = 0; i < count; i++)
        {
            nodes.Add(new TrailNode
            {
                Id = Guid.Empty,
                ModuleId = moduleId,
                ModuleTitle = moduleTitle,
                Title = "",
                Index = i,
                Status = TrailNodeStatus.Locked,
                IsBoss = i == count - 1 && count > 1,
            });
        }
        return nodes;
    }

    // ─── Fase 2: nós por módulo (lazy) ───────────────────────────────────────
    // Concorrência habilitada: vários módulos podem carregar ao mesmo tempo (cada um é
    // idempotente e protegido por LoadState). Sem isso, o AsyncRelayCommand descartaria
    // o carregamento de um módulo enquanto outro estivesse em andamento.
    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task EnsureModuleLoaded(TrailModule module) =>
        EnsureModuleLoadedAsync(module, _cts?.Token ?? CancellationToken.None);

    private async Task EnsureModuleLoadedAsync(TrailModule module, CancellationToken ct)
    {
        if (module is null || module.IsLocked)
            return;
        if (module.LoadState is TrailModuleLoadState.Loading or TrailModuleLoadState.Loaded)
            return;

        module.LoadState = TrailModuleLoadState.Loading;

        try
        {
            var lessons = await _catalog.GetLessonsAsync(module.Id, ct);
            var lessonProgress = await _progress.GetLessonProgressAsync(module.Id, ct);
            if (ct.IsCancellationRequested)
            {
                module.LoadState = TrailModuleLoadState.NotLoaded;
                return;
            }

            var nodes = BuildNodes(module, lessons, lessonProgress);
            module.Nodes = nodes;
            module.TotalCount = nodes.Count;
            module.CompletedCount = nodes.Count(n => n.IsCompleted);
            module.LoadState = TrailModuleLoadState.Loaded;

            // Prefetch do próximo módulo (one-ahead) para suavizar o scroll.
            int idx = Modules.IndexOf(module);
            if (idx >= 0 && idx + 1 < Modules.Count)
                _ = EnsureModuleLoadedAsync(Modules[idx + 1], ct);
        }
        catch (OperationCanceledException)
        {
            module.LoadState = TrailModuleLoadState.NotLoaded;
        }
        catch (HttpRequestException)
        {
            module.LoadState = TrailModuleLoadState.Error;
        }
    }

    private static List<TrailNode> BuildNodes(
        TrailModule module,
        List<LessonResponse> lessons,
        List<LessonProgressResponse> lessonProgress)
    {
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
            if (module.IsLocked)
                status = TrailNodeStatus.Locked;
            else if (completed)
                status = TrailNodeStatus.Completed;
            else if (previousCompleted && !currentAssigned)
            {
                status = TrailNodeStatus.Current;
                currentAssigned = true;
            }
            else
                status = TrailNodeStatus.Locked;

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

        return nodes;
    }

    // ─── Refresh silencioso (substitui o pull-to-refresh) ────────────────────
    private async Task RefreshProgressAsync()
    {
        var ct = ResetCts();
        try
        {
            var overall = await _progress.GetOverallAsync(ct);
            if (ct.IsCancellationRequested) return;

            var progressByModule = overall.Modules.ToDictionary(m => m.ModuleId);
            bool previousCompleted = true;

            foreach (var shell in Modules)
            {
                progressByModule.TryGetValue(shell.Id, out var mp);
                int newTotal = mp?.TotalLessons ?? shell.TotalCount;
                int newCompleted = mp?.CompletedLessons ?? 0;
                bool newLocked = !previousCompleted;

                bool changed = newCompleted != shell.CompletedCount
                    || newTotal != shell.TotalCount
                    || newLocked != shell.IsLocked;

                shell.TotalCount = newTotal;
                shell.CompletedCount = newCompleted;
                shell.IsLocked = newLocked;

                // Invalida módulos cujo progresso mudou para recarregarem os nós ao serem vistos.
                if (changed && !newLocked)
                    shell.LoadState = TrailModuleLoadState.NotLoaded;

                previousCompleted = newTotal > 0 && newCompleted == newTotal;
            }

            _lastSyncUtc = DateTimeOffset.UtcNow;
        }
        catch (OperationCanceledException) { }
        catch (HttpRequestException)
        {
            // Mantém os dados atuais em caso de falha de rede no refresh silencioso.
        }
    }

    // ─── Conclusão de aula: update otimista + reconciliação imediata ─────────
    private void OnProgressChanged(ProgressChangedMessage msg)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Feedback instantâneo: marca a aula concluída na hora (se acertou tudo).
            if (msg.LessonAllCorrect)
                ApplyOptimisticCompletion(msg.ModuleId, msg.LessonId);

            // Reconciliação autoritativa logo em seguida.
            _ = SyncAfterCompletionAsync(msg.ModuleId);
        });
    }

    private void ApplyOptimisticCompletion(Guid moduleId, Guid lessonId)
    {
        var module = Modules.FirstOrDefault(m => m.Id == moduleId);
        if (module is null || module.LoadState != TrailModuleLoadState.Loaded)
            return;

        var nodes = module.Nodes.ToList();
        var idx = nodes.FindIndex(n => n.Id == lessonId);
        if (idx < 0 || nodes[idx].IsCompleted)
            return;

        nodes[idx] = CloneWithStatus(nodes[idx], TrailNodeStatus.Completed);

        // O primeiro nó não concluído vira "atual"; os demais ficam bloqueados.
        bool currentAssigned = false;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].IsCompleted) continue;
            nodes[i] = CloneWithStatus(nodes[i], currentAssigned ? TrailNodeStatus.Locked : TrailNodeStatus.Current);
            currentAssigned = true;
        }

        module.Nodes = nodes;
        module.CompletedCount = nodes.Count(n => n.IsCompleted);
    }

    private static TrailNode CloneWithStatus(TrailNode n, TrailNodeStatus status) => new()
    {
        Id = n.Id,
        ModuleId = n.ModuleId,
        ModuleTitle = n.ModuleTitle,
        Title = n.Title,
        Description = n.Description,
        AudioMediaId = n.AudioMediaId,
        Index = n.Index,
        Status = status,
        Emoji = n.Emoji,
        IsBoss = n.IsBoss,
    };

    private async Task SyncAfterCompletionAsync(Guid moduleId)
    {
        await RefreshProgressAsync();

        var ct = _cts?.Token ?? CancellationToken.None;

        int idx = -1;
        for (int i = 0; i < Modules.Count; i++)
            if (Modules[i].Id == moduleId) { idx = i; break; }
        if (idx < 0) return;

        // Recarrega os nós reais do módulo afetado e do próximo (que pode ter desbloqueado).
        await EnsureModuleLoadedAsync(Modules[idx], ct);
        if (idx + 1 < Modules.Count)
            await EnsureModuleLoadedAsync(Modules[idx + 1], ct);
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
    }
}
