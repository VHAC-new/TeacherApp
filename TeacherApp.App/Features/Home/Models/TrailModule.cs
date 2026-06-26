using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TeacherApp.App.Features.Home.Models;

/// <summary>Estado de carregamento (lazy) do conteúdo de um módulo/trilha.</summary>
public enum TrailModuleLoadState
{
    /// <summary>Shell criado (cabeçalho/contagem/cadeado conhecidos), conteúdo ainda não buscado.</summary>
    NotLoaded,
    Loading,
    Loaded,
    Error,
}

/// <summary>
/// Um módulo ("oceano") da jornada gamificada. Contém um cabeçalho com progresso agregado e
/// uma ou mais <see cref="Trail"/> (trilhas), cada uma com seu próprio caminho de aulas.
/// É um shell observável: cabeçalho/contagem/cadeado vêm do progresso geral (barato) e as
/// trilhas são carregadas sob demanda quando o módulo entra na viewport.
/// </summary>
public sealed partial class TrailModule : ObservableObject
{
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

    /// <summary>Trilhas do módulo. Vazio enquanto <see cref="LoadState"/> não for <see cref="TrailModuleLoadState.Loaded"/>.</summary>
    [ObservableProperty]
    private IReadOnlyList<Trail> _trails = [];

    [ObservableProperty]
    private TrailModuleLoadState _loadState = TrailModuleLoadState.NotLoaded;

    /// <summary>Mensagem de erro do último carregamento (preenchida quando <see cref="LoadState"/> é Error).</summary>
    [ObservableProperty]
    private string? _loadError;

    /// <summary>Módulo bloqueado porque o módulo anterior não foi concluído.</summary>
    [ObservableProperty]
    private bool _isLocked;

    // Contagens agregadas (todas as aulas do módulo) — vêm do progresso geral, antes das trilhas.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressFraction))]
    [NotifyPropertyChangedFor(nameof(CountText))]
    [NotifyPropertyChangedFor(nameof(ProgressPercentText))]
    private int _completedCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressFraction))]
    [NotifyPropertyChangedFor(nameof(CountText))]
    [NotifyPropertyChangedFor(nameof(ProgressPercentText))]
    private int _totalCount;

    public double ProgressFraction => TotalCount > 0 ? (double)CompletedCount / TotalCount : 0;
    public string CountText => $"{CompletedCount}/{TotalCount}";
    public string ProgressPercentText => $"{(int)System.Math.Round(ProgressFraction * 100)}%";

    public string HeaderLabel => $"OCEANO {Order} · {Level.ToUpperInvariant()}";
}
