namespace TeacherApp.App.Core;

/// <summary>
/// Animações reutilizáveis de feedback para o avatar (e outros elementos).
/// </summary>
public static class AnimationExtensions
{
    /// <summary>Chacoalha o elemento na horizontal — usado em resposta errada.</summary>
    public static async Task ShakeAsync(this VisualElement v)
    {
        const uint d = 45;
        await v.TranslateTo(-12, 0, d, Easing.Linear);
        await v.TranslateTo(12, 0, d, Easing.Linear);
        await v.TranslateTo(-9, 0, d, Easing.Linear);
        await v.TranslateTo(9, 0, d, Easing.Linear);
        await v.TranslateTo(-5, 0, d, Easing.Linear);
        await v.TranslateTo(0, 0, d, Easing.Linear);
    }

    /// <summary>Dá um pulo elástico — usado em resposta correta.</summary>
    public static async Task BounceAsync(this VisualElement v)
    {
        await v.ScaleTo(1.22, 150, Easing.CubicOut);
        await v.ScaleTo(1, 380, Easing.SpringOut);
    }
}
