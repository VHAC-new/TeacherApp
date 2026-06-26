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

namespace TeacherApp.App.Features.Home.ViewModels;

public enum LessonsPageState { Loading, Ready, Error }

public partial class LessonsViewModel : ObservableObject, ICleanup
{
    // Janela em que uma troca de aba reutiliza a memória sem rebater na API.
    private static readonly TimeSpan TabSwitchTtl = TimeSpan.FromSeconds(45);

    private readonly TrailRepository _repository;
    private readonly AppSessionState _appSession;

    private CancellationTokenSource? _cts;
    private bool _hasLoaded;
    private DateTimeOffset _lastSyncUtc;

    public LessonsViewModel(TrailRepository repository, AppSessionState appSession)
    {
        _repository = repository;
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
            var shells = await _repository.GetModuleShellsAsync(ct);
            if (ct.IsCancellationRequested) return;

            Modules.Clear();
            foreach (var shell in shells)
                Modules.Add(shell);

            // Carrega as trilhas do primeiro módulo antes de exibir (evita módulo vazio).
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

    // ─── Fase 2: trilhas (+ aulas) por módulo (lazy) ─────────────────────────
    // Concorrência habilitada: vários módulos podem carregar ao mesmo tempo (cada um é
    // idempotente e protegido por LoadState). Sem isso, o AsyncRelayCommand descartaria
    // o carregamento de um módulo enquanto outro estivesse em andamento.
    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task EnsureModuleLoaded(TrailModule module) =>
        EnsureModuleLoadedAsync(module, _cts?.Token ?? CancellationToken.None);

    private async Task EnsureModuleLoadedAsync(TrailModule module, CancellationToken ct)
    {
        // Módulos bloqueados também carregam: a trilha/aulas aparecem com cadeado.
        if (module is null)
            return;
        if (module.LoadState is TrailModuleLoadState.Loading or TrailModuleLoadState.Loaded)
            return;

        module.LoadState = TrailModuleLoadState.Loading;

        try
        {
            var trails = await _repository.GetModuleTrailsAsync(module, ct);
            module.Trails = trails;
            module.CompletedCount = trails.Sum(t => t.CompletedCount);
            module.TotalCount = trails.Sum(t => t.TotalCount);
            module.LoadError = null;
            module.LoadState = TrailModuleLoadState.Loaded;

            // Prefetch encadeado do próximo módulo (one-ahead). Continua MESMO através de módulos
            // bloqueados: assim todo módulo é carregado por este caminho confiável, sem depender do
            // disparo por viewport (que não reativa um item já reciclado). Cada load bloqueado é barato
            // (nós sintéticos, sem buscar aulas), então a cadeia completa é segura.
            int idx = Modules.IndexOf(module);
            if (idx >= 0 && idx + 1 < Modules.Count)
                _ = EnsureModuleLoadedAsync(Modules[idx + 1], ct);
        }
        catch (OperationCanceledException)
        {
            // Carregamento substituído/cancelado: volta a NotLoaded para recarregar ao ser visto.
            module.LoadState = TrailModuleLoadState.NotLoaded;
        }
        catch (Exception ex)
        {
            // Qualquer outra falha (rede, desserialização, dados inconsistentes) vira estado de erro
            // visível com retry — em vez de spinner infinito.
            module.LoadError = ex.Message;
            module.LoadState = TrailModuleLoadState.Error;
        }
    }

    // ─── Refresh silencioso (substitui o pull-to-refresh) ────────────────────
    private async Task RefreshProgressAsync()
    {
        var ct = ResetCts();
        try
        {
            var progressByModule = await _repository.GetOverallProgressAsync(ct);
            if (ct.IsCancellationRequested) return;

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

                // Invalida módulos cujo progresso/bloqueio mudou para recarregarem as trilhas ao
                // serem vistos (inclui os que acabaram de desbloquear: nós sintéticos → aulas reais).
                if (changed)
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
                ApplyOptimisticCompletion(msg.ModuleId, msg.TrailId, msg.LessonId);

            // Reconciliação autoritativa logo em seguida.
            _ = SyncAfterCompletionAsync(msg.ModuleId);
        });
    }

    private void ApplyOptimisticCompletion(Guid moduleId, Guid trailId, Guid lessonId)
    {
        var module = Modules.FirstOrDefault(m => m.Id == moduleId);
        if (module is null || module.LoadState != TrailModuleLoadState.Loaded)
            return;

        var trail = module.Trails.FirstOrDefault(t => t.Id == trailId);
        if (trail is null || trail.LoadState != TrailModuleLoadState.Loaded)
            return;

        var nodes = trail.Nodes.ToList();
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

        trail.Nodes = nodes;
        trail.CompletedCount = nodes.Count(n => n.IsCompleted);
        module.CompletedCount = module.Trails.Sum(t => t.CompletedCount);
    }

    private static TrailNode CloneWithStatus(TrailNode n, TrailNodeStatus status) => new()
    {
        Id = n.Id,
        TrailId = n.TrailId,
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

        // Recarrega as trilhas/nós reais do módulo afetado e do próximo (que pode ter desbloqueado).
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
            $"lesson?moduleId={node.ModuleId}&trailId={node.TrailId}&lessonId={node.Id}&title={Uri.EscapeDataString(node.Title)}&description={desc}&moduleTitle={moduleTitle}{audio}");
    }

    public void Cleanup()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }
}
