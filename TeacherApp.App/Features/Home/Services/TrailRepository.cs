using System.Collections.Generic;
using System.Linq;
using TeacherApp.App.Core.Services;
using TeacherApp.App.Features.Home.Models;
using TeacherApp.Contracts.Lessons;
using TeacherApp.Contracts.Modules;
using TeacherApp.Contracts.Progress;
using TeacherApp.Contracts.Trails;

namespace TeacherApp.App.Features.Home.Services;

/// <summary>
/// Busca catálogo + progresso na API e os converte em módulos/trilhas/nós.
/// Concentra fetch + build + paleta, deixando o <c>LessonsViewModel</c> focado em
/// estado e orquestração. Hierarquia: Módulo → Trilhas → Aulas (nós).
/// </summary>
public sealed class TrailRepository(CatalogService catalog, ProgressService progress)
{
    // Emojis ilustrativos por aula (a API não fornece ícone por aula).
    private static readonly string[] LessonEmojis =
        ["👋", "💬", "🔢", "💡", "🎨", "👨‍👩‍👧", "⭐", "🍎", "🍽️", "🚌", "🛒", "☀️", "📘", "✏️", "🔤", "🗣️"];

    // Paleta por módulo (gradiente do cabeçalho + cor de destaque, herdada pelas trilhas).
    private static readonly (string From, string To, string Accent, string Emoji)[] Palette =
    [
        ("#e11d48", "#fb7185", "#f43f5e", "🌊"),
        ("#0284c7", "#38bdf8", "#0ea5e9", "🏝️"),
        ("#059669", "#34d399", "#10b981", "🏞️"),
        ("#d97706", "#fbbf24", "#f59e0b", "🏔️"),
    ];

    private static readonly string[] Levels =
        ["Iniciante", "Básico", "Intermediário", "Avançado"];

    /// <summary>
    /// Fase 1 (barata): busca <c>GET /modules</c> + <c>GET /progress</c> e monta os shells de módulo
    /// (cabeçalho/contagem agregada/cadeado conhecidos). As trilhas são carregadas sob demanda
    /// quando o módulo entra na viewport (ver <see cref="GetModuleTrailsAsync"/>).
    /// </summary>
    public async Task<IReadOnlyList<TrailModule>> GetModuleShellsAsync(CancellationToken ct)
    {
        var modules = (await catalog.GetModulesAsync(ct)).OrderBy(m => m.Order).ToList();
        var overall = await progress.GetOverallAsync(ct);
        ct.ThrowIfCancellationRequested();

        var progressByModule = overall.Modules.ToDictionary(m => m.ModuleId);

        var shells = new List<TrailModule>(modules.Count);
        bool previousCompleted = true;

        for (int mi = 0; mi < modules.Count; mi++)
        {
            var module = modules[mi];
            progressByModule.TryGetValue(module.Id, out var mp);
            int total = mp?.TotalLessons ?? 0;
            int completed = mp?.CompletedLessons ?? 0;
            bool locked = !previousCompleted;

            shells.Add(BuildShell(module, mi, locked, total, completed));

            previousCompleted = total > 0 && completed == total;
        }

        return shells;
    }

    /// <summary>Fase 2 (lazy): busca as trilhas do módulo + progresso e, para cada trilha desbloqueada,
    /// suas aulas, montando os nós. Trilhas têm trava sequencial dentro do módulo.</summary>
    public async Task<IReadOnlyList<Trail>> GetModuleTrailsAsync(TrailModule module, CancellationToken ct)
    {
        var trails = (await catalog.GetTrailsAsync(module.Id, ct)).OrderBy(t => t.Order).ToList();
        var trailProgress = await progress.GetTrailProgressAsync(module.Id, ct);
        ct.ThrowIfCancellationRequested();

        var progressByTrail = trailProgress.ToDictionary(t => t.TrailId);

        var result = new List<Trail>(trails.Count);
        bool previousCompleted = true;

        foreach (var t in trails)
        {
            progressByTrail.TryGetValue(t.Id, out var tp);
            int total = tp?.TotalLessons ?? t.LessonCount;
            int completed = tp?.CompletedLessons ?? 0;
            bool locked = module.IsLocked || !previousCompleted;

            var trail = BuildTrail(module, t, locked, total, completed);

            // Busca as aulas mesmo quando bloqueada — assim cada nó exibe o nome da lição junto do
            // cadeado. O progresso por aula só importa quando desbloqueada (bloqueada => tudo travado),
            // então evitamos essa chamada extra nesse caso.
            var lessons = await catalog.GetLessonsAsync(t.Id, ct);
            var lessonProgress = locked
                ? new List<LessonProgressResponse>()
                : await progress.GetLessonProgressAsync(t.Id, ct);
            ct.ThrowIfCancellationRequested();

            var nodes = BuildNodes(module, trail, lessons, lessonProgress);
            trail.Nodes = nodes;
            trail.TotalCount = nodes.Count;
            trail.CompletedCount = nodes.Count(n => n.IsCompleted);
            trail.LoadState = TrailModuleLoadState.Loaded;

            result.Add(trail);
            previousCompleted = total > 0 && completed == total;
        }

        return result;
    }

    /// <summary>Refresh silencioso: progresso geral por módulo (sem buscar trilhas), indexado por id.</summary>
    public async Task<IReadOnlyDictionary<Guid, ModuleProgressResponse>> GetOverallProgressAsync(CancellationToken ct)
    {
        var overall = await progress.GetOverallAsync(ct);
        return overall.Modules.ToDictionary(m => m.ModuleId);
    }

    // ─── Build helpers ───────────────────────────────────────────────────────

    private static TrailModule BuildShell(ModuleResponse module, int moduleIndex, bool locked, int total, int completed)
    {
        var palette = Palette[moduleIndex % Palette.Length];
        var level = Levels[Math.Min(moduleIndex, Levels.Length - 1)];

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
            IsLocked = locked,
            TotalCount = total,
            CompletedCount = completed,
            LoadState = TrailModuleLoadState.NotLoaded,
        };
    }

    private static Trail BuildTrail(TrailModule module, TrailResponse trail, bool locked, int total, int completed) =>
        new()
        {
            Id = trail.Id,
            ModuleId = module.Id,
            Title = trail.Title,
            Subtitle = trail.Description,
            Order = trail.Order,
            AccentColor = module.AccentColor,
            Emoji = module.Emoji,
            IsLocked = locked,
            TotalCount = total,
            CompletedCount = completed,
            LoadState = TrailModuleLoadState.NotLoaded,
        };

    private static List<TrailNode> BuildNodes(
        TrailModule module,
        Trail trail,
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
            if (trail.IsLocked)
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
                TrailId = trail.Id,
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
}
