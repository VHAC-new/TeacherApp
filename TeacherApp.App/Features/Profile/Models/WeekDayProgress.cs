using Microsoft.Maui.Graphics;

namespace TeacherApp.App.Features.Profile.Models;

/// <summary>Um dia na faixa "Sequência desta semana" do perfil.</summary>
public sealed class WeekDayProgress
{
    public required string Label { get; init; }
    public bool Studied { get; init; }

    public Color CircleColor => Studied ? Color.FromArgb("#7c5df7") : Color.FromArgb("#2a2650");
    public Color StrokeColor => Studied ? Color.FromArgb("#7c5df7") : Color.FromArgb("#3d3870");
    public Color LabelColor => Studied ? Color.FromArgb("#c4b8ff") : Color.FromArgb("#5a5490");
}
