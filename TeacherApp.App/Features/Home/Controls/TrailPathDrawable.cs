using System.Collections.Generic;
using Microsoft.Maui.Graphics;

namespace TeacherApp.App.Features.Home.Controls;

/// <summary>
/// Desenha o caminho tracejado em zig-zag que liga os nós da trilha,
/// reproduzindo o estilo do Figma (curvas suaves, trecho concluído em destaque).
/// </summary>
public sealed class TrailPathDrawable : IDrawable
{
    private static readonly Color LockedColor = new(61 / 255f, 56 / 255f, 112 / 255f, 0.7f);
    private static readonly Color LockedDotColor = new(61 / 255f, 56 / 255f, 112 / 255f, 0.5f);

    /// <summary>Frações horizontais (0..1) de cada nó.</summary>
    public IReadOnlyList<double> FracX { get; set; } = [];

    /// <summary>Posição vertical (px) do centro de cada nó.</summary>
    public IReadOnlyList<double> Cy { get; set; } = [];

    /// <summary>Se o nó na posição i está concluído (colore o segmento i→i+1).</summary>
    public IReadOnlyList<bool> NodeCompleted { get; set; } = [];

    public string AccentColor { get; set; } = "#7c5df7";

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        int n = FracX.Count;
        if (n < 2 || Cy.Count != n) return;

        float width = dirtyRect.Width;
        var pts = new PointF[n];
        for (int i = 0; i < n; i++)
            pts[i] = new PointF((float)(FracX[i] * width), (float)Cy[i]);

        var accent = Color.FromArgb(AccentColor).WithAlpha(0.6f);

        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeSize = 6;

        // Caminho completo (bloqueado / cinza tracejado).
        canvas.StrokeColor = LockedColor;
        canvas.StrokeDashPattern = [10, 10];
        canvas.DrawPath(BuildPath(pts, 0, n - 1));

        // Sobreposição do trecho concluído (destaque colorido).
        canvas.StrokeColor = accent;
        for (int i = 0; i < n - 1; i++)
        {
            if (i < NodeCompleted.Count && NodeCompleted[i])
                canvas.DrawPath(BuildPath(pts, i, i + 1));
        }

        // Pontos no meio de cada segmento (sensação de "pegadas").
        canvas.StrokeDashPattern = [];
        for (int i = 0; i < n - 1; i++)
        {
            float mx = (pts[i].X + pts[i + 1].X) / 2f;
            float my = (pts[i].Y + pts[i + 1].Y) / 2f;
            bool done = i < NodeCompleted.Count && NodeCompleted[i];
            canvas.FillColor = done ? accent : LockedDotColor;
            canvas.FillCircle(mx, my, 4);
        }
    }

    /// <summary>Curva cúbica suave entre os nós (controle vertical a 50%, como no Figma).</summary>
    private static PathF BuildPath(PointF[] pts, int from, int to)
    {
        var path = new PathF();
        path.MoveTo(pts[from].X, pts[from].Y);
        for (int i = from + 1; i <= to; i++)
        {
            var prev = pts[i - 1];
            var curr = pts[i];
            float midY = prev.Y + (curr.Y - prev.Y) * 0.5f;
            path.CurveTo(prev.X, midY, curr.X, midY, curr.X, curr.Y);
        }
        return path;
    }
}
