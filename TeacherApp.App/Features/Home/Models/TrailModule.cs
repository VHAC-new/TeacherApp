using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TeacherApp.App.Features.Home.Models;

/// <summary>Estado de carregamento (lazy) do conteúdo de um módulo.</summary>
public enum TrailModuleLoadState
{
    /// <summary>Shell criado (cabeçalho/contagem/cadeado conhecidos), nós ainda não buscados.</summary>
    NotLoaded,
    Loading,
    Loaded,
    Error,
}

/// <summary>
/// Um módulo ("mundo") da trilha gamificada, com seus nós (lições) e metadados de progresso.
/// É um shell observável: cabeçalho/contagem/cadeado vêm do progresso geral (barato) e os nós
/// são carregados sob demanda quando o módulo entra na viewport.
/// </summary>
public sealed partial class TrailModule : ObservableObject
{
    // Espaçamento do layout em zig-zag (em unidades independentes de dispositivo).
    private const double FirstNodeY = 60;
    private const double NodeSpacingY = 130;
    private const double BottomPadding = 140;

    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public string? Subtitle { get; init; }
    public required string Level { get; init; }
    public int Order { get; init; }

    /// <summary>Cores do gradiente do card de cabeçalho (paleta por módulo).</summary>
    public required string GradientFrom { get; init; }
    public required string GradientTo { get; init; }
    public required string AccentColor { get; init; }
    public required string Emoji { get; init; }

    /// <summary>Nós (lições). Vazio enquanto <see cref="LoadState"/> não for <see cref="TrailModuleLoadState.Loaded"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentIndex))]
    [NotifyPropertyChangedFor(nameof(ActiveTrailTitle))]
    private IReadOnlyList<TrailNode> _nodes = [];

    [ObservableProperty]
    private TrailModuleLoadState _loadState = TrailModuleLoadState.NotLoaded;

    /// <summary>Módulo bloqueado porque o módulo anterior não foi concluído.</summary>
    [ObservableProperty]
    private bool _isLocked;

    // Contagens vêm do progresso geral (OverallProgressResponse) — disponíveis antes dos nós.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressFraction))]
    [NotifyPropertyChangedFor(nameof(CountText))]
    [NotifyPropertyChangedFor(nameof(ProgressPercentText))]
    private int _completedCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressFraction))]
    [NotifyPropertyChangedFor(nameof(CountText))]
    [NotifyPropertyChangedFor(nameof(ProgressPercentText))]
    [NotifyPropertyChangedFor(nameof(TrailHeight))]
    private int _totalCount;

    public double ProgressFraction => TotalCount > 0 ? (double)CompletedCount / TotalCount : 0;
    public string CountText => $"{CompletedCount}/{TotalCount}";
    public string ProgressPercentText => $"{(int)System.Math.Round(ProgressFraction * 100)}%";

    public string HeaderLabel => $"MÓDULO {Order} · {Level.ToUpperInvariant()}";

    /// <summary>Índice do nó atual; -1 se não houver (tudo concluído, bloqueado ou ainda não carregado).</summary>
    public int CurrentIndex => Nodes.ToList().FindIndex(n => n.IsCurrent);

    /// <summary>Título destacado da trilha ativa (primeira lição não concluída).</summary>
    public string ActiveTrailTitle =>
        Nodes.FirstOrDefault(n => !n.IsCompleted)?.Title
        ?? Nodes.LastOrDefault()?.Title
        ?? Title;

    /// <summary>Altura da área da trilha para o AbsoluteLayout — derivada de <see cref="TotalCount"/>,
    /// então conhecida antes mesmo dos nós carregarem (mantém o scroll estável).</summary>
    public double TrailHeight => FirstNodeY + System.Math.Max(0, TotalCount - 1) * NodeSpacingY + BottomPadding;
}
