using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using TeacherApp.App.Features.Home.Models;

namespace TeacherApp.App.Features.Home.Controls;

/// <summary>
/// Renderiza um módulo ("mundo") como trilha gamificada (estilo Figma):
/// card de cabeçalho + caminho zig-zag tracejado + nós (lições) + mascote.
/// Módulos bloqueados são exibidos como um card recolhido.
/// </summary>
public sealed class TrailView : ContentView
{
    private const string MotivationalText = "Vamos aprender hoje!";

    public static readonly BindableProperty NodeCommandProperty =
        BindableProperty.Create(nameof(NodeCommand), typeof(ICommand), typeof(TrailView));

    public ICommand? NodeCommand
    {
        get => (ICommand?)GetValue(NodeCommandProperty);
        set => SetValue(NodeCommandProperty, value);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        Content = BindingContext is TrailModule module
            ? (module.IsLocked ? BuildLockedCard(module) : BuildActive(module))
            : null;
    }

    // ─── Layout helpers ──────────────────────────────────────────────────────

    private static double FracOf(int index) => (index % 3) switch
    {
        0 => 0.5,
        1 => 0.22,
        _ => 0.76,
    };

    private static double CyOf(int index) => 60 + index * 130;

    private static Color C(string hex) => Color.FromArgb(hex);

    /// <summary>Escurece uma cor multiplicando os canais RGB (mantém o alfa) — usado na base 3D.</summary>
    private static Color Darken(Color c, float factor) =>
        new(c.Red * factor, c.Green * factor, c.Blue * factor, c.Alpha);

    /// <summary>Flutuação contínua (sobe/desce suave) — efeito "float" do nó atual.</summary>
    private static void StartFloat(VisualElement target)
    {
        var anim = new Animation
        {
            { 0.0, 0.5, new Animation(v => target.TranslationY = v, 0, -8, Easing.SinInOut) },
            { 0.5, 1.0, new Animation(v => target.TranslationY = v, -8, 0, Easing.SinInOut) },
        };
        target.Animate("nodeFloat", anim, length: 2400, repeat: () => true);
    }

    // ─── Locked module card ──────────────────────────────────────────────────

    private static View BuildLockedCard(TrailModule m)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
            ColumnSpacing = 12,
            VerticalOptions = LayoutOptions.Center,
        };

        var lockLabel = new Label
        {
            Text = "🔒",
            FontSize = 18,
            VerticalOptions = LayoutOptions.Center,
        };
        grid.Add(lockLabel, 0, 0);

        var texts = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        texts.Add(new Label { Text = m.Title, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Colors.White });
        if (!string.IsNullOrWhiteSpace(m.Subtitle))
            texts.Add(new Label { Text = m.Subtitle, FontSize = 12, TextColor = C("#5a5490") });
        grid.Add(texts, 1, 0);

        return new Border
        {
            Margin = new Thickness(16, 8),
            Padding = 16,
            Opacity = 0.55,
            BackgroundColor = C("#1e1c3a"),
            Stroke = C("#3d3870"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Content = grid,
        };
    }

    // ─── Active module ───────────────────────────────────────────────────────

    private View BuildActive(TrailModule m)
    {
        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Add(BuildHeaderCard(m));
        stack.Add(BuildTrailArea(m));
        return stack;
    }

    private static View BuildHeaderCard(TrailModule m)
    {
        var accent = C(m.AccentColor);

        var headerBrush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
            GradientStops =
            {
                new GradientStop(C(m.GradientFrom).WithAlpha(0.45f), 0f),
                new GradientStop(C(m.GradientTo).WithAlpha(0.22f), 1f),
            },
        };

        // Linha 1: emoji + (módulo/level + título) + contador
        var emojiChip = new Border
        {
            WidthRequest = 36,
            HeightRequest = 36,
            BackgroundColor = accent.WithAlpha(0.3f),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Content = new Label
            {
                Text = m.Emoji,
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            },
        };

        var titleStack = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
        titleStack.Add(new Label
        {
            Text = m.HeaderLabel,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = C("#a78bfa"),
            CharacterSpacing = 1.5,
        });
        titleStack.Add(new Label
        {
            Text = m.Title,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
        });

        var countLabel = new Label
        {
            Text = m.CountText,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = C("#8a85b8"),
            VerticalOptions = LayoutOptions.Center,
        };

        var row1 = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            ColumnSpacing = 12,
        };
        row1.Add(emojiChip, 0, 0);
        row1.Add(titleStack, 1, 0);
        row1.Add(countLabel, 2, 0);

        // Linha 2: destaque "TRILHA ATIVA" + percentual
        var activeTexts = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        activeTexts.Add(new Label
        {
            Text = "TRILHA ATIVA",
            FontSize = 10,
            FontAttributes = FontAttributes.Bold,
            TextColor = C("#c4b5fd"),
            CharacterSpacing = 1.5,
        });
        activeTexts.Add(new Label
        {
            Text = m.ActiveTrailTitle,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
        });
        if (!string.IsNullOrWhiteSpace(m.Subtitle))
            activeTexts.Add(new Label { Text = m.Subtitle, FontSize = 12, TextColor = C("#c4b8ff") });

        var percentCircle = new Border
        {
            WidthRequest = 38,
            HeightRequest = 38,
            BackgroundColor = accent,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 19 },
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = m.ProgressPercentText,
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            },
        };

        var row2Grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
            },
            ColumnSpacing = 10,
        };
        row2Grid.Add(activeTexts, 0, 0);
        row2Grid.Add(percentCircle, 1, 0);

        var row2 = new Border
        {
            Margin = new Thickness(0, 12, 0, 0),
            Padding = new Thickness(12, 10),
            BackgroundColor = accent.WithAlpha(0.5f),
            Stroke = Colors.White.WithAlpha(0.15f),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Content = row2Grid,
        };

        var inner = new VerticalStackLayout { Spacing = 0 };
        inner.Add(row1);
        inner.Add(row2);

        return new Border
        {
            Margin = new Thickness(16, 8, 16, 4),
            Padding = 16,
            Background = headerBrush,
            Stroke = accent.WithAlpha(0.4f),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Content = inner,
        };
    }

    private View BuildTrailArea(TrailModule m)
    {
        var abs = new AbsoluteLayout { HeightRequest = m.TrailHeight };

        // Caminho tracejado (fica atrás dos nós).
        var drawable = new TrailPathDrawable
        {
            FracX = m.Nodes.Select(n => FracOf(n.Index)).ToList(),
            Cy = m.Nodes.Select(n => CyOf(n.Index)).ToList(),
            NodeCompleted = m.Nodes.Select(n => n.IsCompleted).ToList(),
            AccentColor = m.AccentColor,
        };
        var graphics = new GraphicsView { Drawable = drawable, InputTransparent = true };
        AbsoluteLayout.SetLayoutBounds(graphics, new Rect(0, 0, 1, 1));
        AbsoluteLayout.SetLayoutFlags(graphics, AbsoluteLayoutFlags.All);
        abs.Add(graphics);

        // Nós.
        foreach (var node in m.Nodes)
        {
            var container = BuildNode(m, node);
            double bubbleSize = node.IsBoss ? 72 : node.IsCurrent ? 68 : 60;
            double pillOffset = node.IsCurrent ? 32 : 0;
            double topY = CyOf(node.Index) - (pillOffset + bubbleSize / 2);

            AbsoluteLayout.SetLayoutBounds(container, new Rect(FracOf(node.Index), topY, 100, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(container, AbsoluteLayoutFlags.XProportional);
            abs.Add(container);
        }

        // Mascote junto ao nó atual.
        int curIdx = m.CurrentIndex;
        if (curIdx >= 0)
        {
            var mascot = BuildMascot();
            double mFrac = FracOf(curIdx) switch
            {
                0.22 => 0.66,
                0.76 => 0.30,
                _ => 0.85,
            };
            AbsoluteLayout.SetLayoutBounds(mascot, new Rect(mFrac, CyOf(curIdx) - 44, 120, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(mascot, AbsoluteLayoutFlags.XProportional);
            abs.Add(mascot);
        }

        return abs;
    }

    private View BuildNode(TrailModule m, TrailNode node)
    {
        var accent = C(m.AccentColor);
        double bubbleSize = node.IsBoss ? 72 : node.IsCurrent ? 68 : 60;

        var container = new VerticalStackLayout
        {
            Spacing = 6,
            WidthRequest = 100,
            HorizontalOptions = LayoutOptions.Center,
        };

        // Grupo que "flutua" (pílula + bolha) quando o nó é o atual.
        var nodeGroup = new VerticalStackLayout
        {
            Spacing = 6,
            HorizontalOptions = LayoutOptions.Center,
        };

        // Pílula "COMEÇAR" acima do nó atual.
        if (node.IsCurrent)
        {
            nodeGroup.Add(new Border
            {
                Padding = new Thickness(12, 4),
                BackgroundColor = Colors.White,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                HorizontalOptions = LayoutOptions.Center,
                Content = new Label
                {
                    Text = "COMEÇAR",
                    FontSize = 11,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = accent,
                    CharacterSpacing = 0.5,
                },
            });
        }

        // Bolha. mainColor define a face; a base é uma versão mais escura (efeito 3D).
        Brush bubbleBrush;
        Color bubbleBorder;
        Color mainColor;
        View bubbleContent;

        if (node.IsCompleted)
        {
            mainColor = C("#10b981");
            bubbleBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops =
                {
                    new GradientStop(C("#10b981"), 0f),
                    new GradientStop(C("#10b981").WithAlpha(0.8f), 1f),
                },
            };
            bubbleBorder = C("#34d399");
            bubbleContent = new Label { Text = "✓", FontSize = 26, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        }
        else if (node.IsCurrent)
        {
            mainColor = accent;
            bubbleBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops =
                {
                    new GradientStop(accent, 0f),
                    new GradientStop(accent.WithAlpha(0.8f), 1f),
                },
            };
            bubbleBorder = Colors.White;
            bubbleContent = new Label { Text = node.Emoji, FontSize = 24, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        }
        else // locked
        {
            mainColor = C("#2a2650");
            bubbleBrush = new SolidColorBrush(C("#2a2650"));
            bubbleBorder = C("#3d3870");
            bubbleContent = new Label { Text = node.IsBoss ? "🏆" : "🔒", FontSize = 20, TextColor = C("#5a5490"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        }

        const double offsetY = 6;

        // Face superior (botão clicável).
        var bubble = new Border
        {
            WidthRequest = bubbleSize,
            HeightRequest = bubbleSize,
            Background = bubbleBrush,
            Stroke = bubbleBorder,
            StrokeThickness = 3,
            StrokeShape = new RoundRectangle { CornerRadius = bubbleSize / 2 },
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            Content = bubbleContent,
        };

        // Base sólida mais escura, deslocada para baixo → sombra 3D "pressionável"
        // (estilo kernel-ai/Duolingo: dois círculos empilhados, sem blur).
        var baseCircle = new Border
        {
            WidthRequest = bubbleSize,
            HeightRequest = bubbleSize,
            BackgroundColor = Darken(mainColor, 0.72f),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = bubbleSize / 2 },
            Margin = new Thickness(0, offsetY, 0, 0),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
        };

        var holder = new Grid
        {
            WidthRequest = bubbleSize,
            HeightRequest = bubbleSize + offsetY,
            HorizontalOptions = LayoutOptions.Center,
        };
        holder.Add(baseCircle);
        holder.Add(bubble);

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) =>
        {
            // Afunda a face até a base e volta, depois navega.
            await bubble.TranslateTo(0, offsetY, 70, Easing.CubicIn);
            await bubble.TranslateTo(0, 0, 70, Easing.CubicOut);

            var cmd = NodeCommand;
            if (cmd?.CanExecute(node) == true)
                cmd.Execute(node);
        };
        holder.GestureRecognizers.Add(tap);
        nodeGroup.Add(holder);
        container.Add(nodeGroup);

        // Flutuação contínua do nó atual (igual ao kernel-ai/Figma).
        if (node.IsCurrent)
        {
            nodeGroup.Loaded += (_, _) => StartFloat(nodeGroup);
            nodeGroup.Unloaded += (_, _) => nodeGroup.AbortAnimation("nodeFloat");
        }

        // Rótulo abaixo (apenas para nós não bloqueados, como no Figma).
        if (!node.IsLocked)
        {
            container.Add(new Label
            {
                Text = node.Title,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = C("#c4b8ff"),
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 2,
            });
        }

        return container;
    }

    private static View BuildMascot()
    {
        var stack = new VerticalStackLayout
        {
            Spacing = 4,
            WidthRequest = 120,
            HorizontalOptions = LayoutOptions.Center,
            InputTransparent = true,
        };

        stack.Add(new Border
        {
            Padding = new Thickness(8, 5),
            BackgroundColor = C("#7c5df7").WithAlpha(0.85f),
            Stroke = Colors.White.WithAlpha(0.2f),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            HorizontalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = MotivationalText,
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
            },
        });

        stack.Add(new Image
        {
            Source = "wolf_mascot.png",
            WidthRequest = 52,
            HeightRequest = 52,
            HorizontalOptions = LayoutOptions.Center,
        });

        return stack;
    }
}
