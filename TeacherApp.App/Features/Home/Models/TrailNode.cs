namespace TeacherApp.App.Features.Home.Models;

public enum TrailNodeStatus
{
    Completed,
    Current,
    Locked,
}

/// <summary>
/// Um nó da trilha (uma lição) no estilo gamificado do Figma.
/// </summary>
public sealed class TrailNode
{
    public required Guid Id { get; init; }
    public required Guid TrailId { get; init; }
    public required Guid ModuleId { get; init; }
    public required string ModuleTitle { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public Guid? AudioMediaId { get; init; }

    /// <summary>Posição na trilha (0-based) usada para o layout em zig-zag.</summary>
    public int Index { get; init; }

    public TrailNodeStatus Status { get; init; }

    /// <summary>Emoji ilustrativo do nó (a API não fornece ícone por lição).</summary>
    public string Emoji { get; init; } = "📘";

    /// <summary>Último nó do módulo (recebe troféu quando bloqueado, como no Figma).</summary>
    public bool IsBoss { get; init; }

    public bool IsCompleted => Status == TrailNodeStatus.Completed;
    public bool IsCurrent => Status == TrailNodeStatus.Current;
    public bool IsLocked => Status == TrailNodeStatus.Locked;
}
