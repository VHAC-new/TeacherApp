using Microsoft.Maui.Layouts;

namespace TeacherApp.App.Core.Controls;

/// <summary>
/// Overlay leve de confete desenhado em runtime (sem Lottie/SkiaSharp).
/// Dispara um burst de partículas coloridas que voam para fora, caem com
/// "gravidade", giram e somem. Não bloqueia toques (InputTransparent).
/// </summary>
public sealed class ConfettiView : ContentView
{
    private static readonly Color[] Palette =
    [
        Color.FromArgb("#7C5DF7"),
        Color.FromArgb("#FBBF24"),
        Color.FromArgb("#10B981"),
        Color.FromArgb("#C4B8FF"),
        Color.FromArgb("#F472B6"),
    ];

    private readonly AbsoluteLayout _layout = new();
    private readonly Random _rand = new();

    public ConfettiView()
    {
        InputTransparent = true;
        Content = _layout;
    }

    /// <summary>
    /// Dispara um burst de confete. <paramref name="origin"/> é relativo à própria
    /// view; quando nulo usa o centro. Aguarda o fim da animação antes de retornar.
    /// </summary>
    public async Task BurstAsync(Point? origin = null, int count = 22, uint durationMs = 950)
    {
        var center = origin ?? new Point(
            Width > 0 ? Width / 2 : 120,
            Height > 0 ? Height / 2 : 60);

        for (var i = 0; i < count; i++)
            SpawnParticle(center, durationMs);

        await Task.Delay((int)durationMs + 60);
        _layout.Clear();
    }

    private void SpawnParticle(Point center, uint durationMs)
    {
        var size = 6 + _rand.Next(9);
        var particle = new BoxView
        {
            Color = Palette[_rand.Next(Palette.Length)],
            WidthRequest = size,
            HeightRequest = size,
            CornerRadius = _rand.Next(2) == 0 ? size / 2 : 2,
            InputTransparent = true,
        };

        AbsoluteLayout.SetLayoutBounds(particle, new Rect(center.X, center.Y, size, size));
        _layout.Add(particle);

        // Vetor inicial em círculo, com leve viés para cima; gravidade puxa para baixo.
        var angle = _rand.NextDouble() * 2 * Math.PI;
        var speed = 70 + _rand.NextDouble() * 130;
        var dx = Math.Cos(angle) * speed;
        var dy = Math.Sin(angle) * speed - 60;
        const double gravity = 260;
        var spin = (_rand.NextDouble() * 720) - 360;

        var anim = new Animation(t =>
        {
            particle.TranslationX = dx * t;
            particle.TranslationY = dy * t + gravity * t * t;
            particle.Rotation = spin * t;
            particle.Opacity = t < 0.7 ? 1 : 1 - (t - 0.7) / 0.3;
        });

        particle.Animate($"confetti{_rand.Next()}", anim, length: durationMs, easing: Easing.CubicOut);
    }
}
