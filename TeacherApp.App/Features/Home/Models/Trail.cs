using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TeacherApp.App.Features.Home.Models;

/// <summary>
/// Uma trilha ("TRILHA") dentro de um módulo: card + caminho zig-zag de nós (aulas).
/// É a unidade visual renderizada pelo <c>TrailView</c>. Carrega o próprio progresso e
/// estado de cadeado (sequencial entre trilhas do mesmo módulo).
/// </summary>
public sealed partial class Trail : ObservableObject
{
    // Espaçamento do layout em zig-zag (em unidades independentes de dispositivo).
    private const double FirstNodeY = 60;
    private const double NodeSpacingY = 130;
    private const double BottomPadding = 100;

    public required Guid Id { get; init; }
    public required Guid ModuleId { get; init; }
    public required string Title { get; init; }
    public string? Subtitle { get; init; }
    public int Order { get; init; }

    /// <summary>Cor de destaque (herdada do módulo) usada nos nós/caminho.</summary>
    public required string AccentColor { get; init; }
    public required string Emoji { get; init; }

    /// <summary>Nós (aulas). Vazio enquanto <see cref="LoadState"/> não for <see cref="TrailModuleLoadState.Loaded"/>.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentIndex))]
    [NotifyPropertyChangedFor(nameof(ActiveNodeTitle))]
    private IReadOnlyList<TrailNode> _nodes = [];

    [ObservableProperty]
    private TrailModuleLoadState _loadState = TrailModuleLoadState.NotLoaded;

    /// <summary>Trilha bloqueada porque a trilha anterior do módulo não foi concluída.</summary>
    [ObservableProperty]
    private bool _isLocked;

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

    /// <summary>Índice do nó atual; -1 se não houver (tudo concluído, bloqueado ou não carregado).</summary>
    public int CurrentIndex => Nodes.ToList().FindIndex(n => n.IsCurrent);

    /// <summary>Título do nó ativo (primeira aula não concluída).</summary>
    public string ActiveNodeTitle =>
        Nodes.FirstOrDefault(n => !n.IsCompleted)?.Title
        ?? Nodes.LastOrDefault()?.Title
        ?? Title;

    /// <summary>Altura da área da trilha para o AbsoluteLayout — derivada de <see cref="TotalCount"/>,
    /// conhecida antes mesmo dos nós carregarem (mantém o scroll estável).</summary>
    public double TrailHeight => FirstNodeY + System.Math.Max(0, TotalCount - 1) * NodeSpacingY + BottomPadding;
}
