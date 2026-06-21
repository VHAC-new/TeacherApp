using System.Collections.Generic;
using System.Linq;

namespace TeacherApp.App.Features.Home.Models;

/// <summary>
/// Um módulo ("mundo") da trilha gamificada, com seus nós (lições) e metadados de progresso.
/// </summary>
public sealed class TrailModule
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

    public required IReadOnlyList<TrailNode> Nodes { get; init; }

    /// <summary>Módulo bloqueado porque o módulo anterior não foi concluído.</summary>
    public bool IsLocked { get; init; }

    public int TotalCount => Nodes.Count;
    public int CompletedCount => Nodes.Count(n => n.IsCompleted);
    public double ProgressFraction => TotalCount > 0 ? (double)CompletedCount / TotalCount : 0;
    public string CountText => $"{CompletedCount}/{TotalCount}";
    public string ProgressPercentText => $"{(int)System.Math.Round(ProgressFraction * 100)}%";

    public string HeaderLabel => $"MÓDULO {Order} · {Level.ToUpperInvariant()}";

    /// <summary>Índice do nó atual; -1 se não houver (tudo concluído ou bloqueado).</summary>
    public int CurrentIndex => Nodes.ToList().FindIndex(n => n.IsCurrent);

    /// <summary>Título destacado da trilha ativa (primeira lição não concluída).</summary>
    public string ActiveTrailTitle =>
        Nodes.FirstOrDefault(n => !n.IsCompleted)?.Title
        ?? Nodes.LastOrDefault()?.Title
        ?? Title;

    /// <summary>Altura total da área da trilha para o AbsoluteLayout.</summary>
    public double TrailHeight => FirstNodeY + System.Math.Max(0, TotalCount - 1) * NodeSpacingY + BottomPadding;
}
