using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace TeacherApp.App.Features.Profile.Models;

/// <summary>Progresso de um módulo ("mundo") na seção "PROGRESSO POR MUNDO".</summary>
public sealed class WorldProgress
{
    public required string Title { get; init; }
    public int Completed { get; init; }
    public int Total { get; init; }
    public required Color Accent { get; init; }

    public double Fraction => Total > 0 ? (double)Completed / Total : 0;
    public string CountText => $"{Completed}/{Total}";

    // Largura da barra via colunas estreláveis (evita binding de gradiente).
    public GridLength FillStar => new(Fraction, GridUnitType.Star);
    public GridLength RestStar => new(1 - Fraction, GridUnitType.Star);
}
