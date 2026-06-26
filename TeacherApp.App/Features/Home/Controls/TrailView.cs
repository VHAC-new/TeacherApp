using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using TeacherApp.App.Features.Home.Models;

namespace TeacherApp.App.Features.Home.Controls;

/// <summary>
/// Renderiza uma <see cref="Trail"/> (trilha) como caminho gamificado (estilo Figma):
/// card "TRILHA" + caminho zig-zag tracejado + nós (aulas) + mascote.
/// Trilhas bloqueadas exibem o card recolhido com cadeado.
///
/// O esqueleto (card, área da trilha, GraphicsView do caminho) é montado uma vez por
/// instância e reaproveitado a cada reciclagem do StackLayout do módulo: atualizamos os
/// valores in-place e reconstruímos apenas os nós (que variam por trilha).
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

    private Trail? _trail;
    private VisualElement? _floatTarget;

    // ─── Esqueleto reaproveitado (montado uma vez por instância) ──────────────
    private readonly TrailCard _card;
    private readonly ContentView _bodyHost;          // alterna entre placeholder e trilha
    private readonly Grid _placeholderArea;          // corpo durante o carregamento (spinner)
    private readonly ActivityIndicator _placeholderSpinner;
    private readonly AbsoluteLayout _trailArea;      // corpo quando carregado (caminho + nós + mascote)
    private readonly GraphicsView _pathView;         // caminho tracejado (índice 0, sempre presente)
    private readonly TrailPathDrawable _pathDrawable;
    private readonly VerticalStackLayout _root;

    public TrailView()
    {
        _card = new TrailCard();

        _pathDrawable = new TrailPathDrawable();
        _pathView = new GraphicsView { Drawable = _pathDrawable, InputTransparent = true };
        AbsoluteLayout.SetLayoutBounds(_pathView, new Rect(0, 0, 1, 1));
        AbsoluteLayout.SetLayoutFlags(_pathView, AbsoluteLayoutFlags.All);

        _trailArea = new AbsoluteLayout();
        _trailArea.Add(_pathView);

        _placeholderSpinner = new ActivityIndicator
        {
            IsRunning = true,
            WidthRequest = 32,
            HeightRequest = 32,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 32, 0, 0),
        };
        _placeholderArea = new Grid();
        _placeholderArea.Add(_placeholderSpinner);

        _bodyHost = new ContentView();

        _root = new VerticalStackLayout { Spacing = 0 };
        _root.Add(_card.View);
        _root.Add(_bodyHost);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (_trail is not null)
            _trail.PropertyChanged -= OnTrailPropertyChanged;

        _trail = BindingContext as Trail;

        if (_trail is not null)
            _trail.PropertyChanged += OnTrailPropertyChanged;

        Rebuild();
    }

    private void OnTrailPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        bool structural = e.PropertyName is nameof(Trail.LoadState) or nameof(Trail.IsLocked);
        bool contentWhileLoaded =
            e.PropertyName is nameof(Trail.Nodes) or nameof(Trail.CompletedCount)
            && _trail?.LoadState == TrailModuleLoadState.Loaded;

        if (!structural && !contentWhileLoaded)
            return;

        MainThread.BeginInvokeOnMainThread(Rebuild);
    }

    private void Rebuild()
    {
        AbortFloat();

        if (_trail is null)
        {
            Content = null;
            return;
        }

        _card.Update(_trail);

        if (_trail.LoadState == TrailModuleLoadState.Loaded)
        {
            if (!ReferenceEquals(_bodyHost.Content, _trailArea))
                _bodyHost.Content = _trailArea;
            UpdateTrail(_trail);
        }
        else
        {
            _placeholderArea.HeightRequest = _trail.TrailHeight;
            _placeholderSpinner.Color = C(_trail.AccentColor);
            if (!ReferenceEquals(_bodyHost.Content, _placeholderArea))
                _bodyHost.Content = _placeholderArea;
        }

        if (!ReferenceEquals(Content, _root))
            Content = _root;
    }

    private void UpdateTrail(Trail t)
    {
        _trailArea.HeightRequest = t.TrailHeight;

        // Caminho tracejado (reusa o GraphicsView/drawable; só atualiza os dados e redesenha).
        _pathDrawable.FracX = t.Nodes.Select(n => FracOf(n.Index)).ToList();
        _pathDrawable.Cy = t.Nodes.Select(n => CyOf(n.Index)).ToList();
        _pathDrawable.NodeCompleted = t.Nodes.Select(n => n.IsCompleted).ToList();
        _pathDrawable.AccentColor = t.AccentColor;
        _pathView.Invalidate();

        // Remove nós/mascote anteriores, preservando o GraphicsView (índice 0).
        for (int i = _trailArea.Children.Count - 1; i >= 1; i--)
            _trailArea.Children.RemoveAt(i);

        // Nós.
        foreach (var node in t.Nodes)
        {
            var container = BuildNode(t, node);
            double bubbleSize = node.IsBoss ? 72 : node.IsCurrent ? 68 : 60;
            double pillOffset = node.IsCurrent ? 32 : 0;
            double topY = CyOf(node.Index) - (pillOffset + bubbleSize / 2);

            AbsoluteLayout.SetLayoutBounds(container, new Rect(FracOf(node.Index), topY, 100, AbsoluteLayout.AutoSize));
            AbsoluteLayout.SetLayoutFlags(container, AbsoluteLayoutFlags.XProportional);
            _trailArea.Add(container);
        }

        // Mascote junto ao nó atual.
        int curIdx = t.CurrentIndex;
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
            _trailArea.Add(mascot);
        }
    }

    private void AbortFloat()
    {
        _floatTarget?.AbortAnimation("nodeFloat");
        _floatTarget = null;
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

    // Flutuação do nó atual: deslocamento (px) e duração de um ciclo completo (sobe + desce).
    private const double FloatAmplitude = 10;
    private const uint FloatPeriodMs = 1600;

    private static void StartFloat(VisualElement target)
    {
        target.AbortAnimation("nodeFloat");
        var anim = new Animation(
            v => target.TranslationY = -FloatAmplitude * (1 - Math.Cos(v * 2 * Math.PI)) / 2);
        target.Animate("nodeFloat", anim, length: FloatPeriodMs, repeat: () => true);
    }

    // ─── Card "TRILHA" (estrutura fixa, atualizada in-place) ──────────────────

    /// <summary>
    /// Card de cabeçalho da trilha (o card "TRILHA" do Figma). Estrutura montada uma vez
    /// e atualizada via <see cref="Update"/> a cada reciclagem.
    /// </summary>
    private sealed class TrailCard
    {
        public Border View { get; }

        private readonly Border _iconChip;
        private readonly Label _icon;
        private readonly Label _label;
        private readonly Label _title;
        private readonly Label _subtitle;
        private readonly Border _countBadge;
        private readonly Label _count;

        public TrailCard()
        {
            _icon = new Label
            {
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };
            _iconChip = new Border
            {
                WidthRequest = 40,
                HeightRequest = 40,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                Content = _icon,
            };

            _label = new Label
            {
                Text = "TRILHA",
                FontSize = 10,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White.WithAlpha(0.75f),
                CharacterSpacing = 1.5,
            };
            _title = new Label
            {
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
            };
            _subtitle = new Label
            {
                FontSize = 12,
                TextColor = Colors.White.WithAlpha(0.8f),
                LineBreakMode = LineBreakMode.TailTruncation,
                MaxLines = 1,
            };
            var texts = new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center };
            texts.Add(_label);
            texts.Add(_title);
            texts.Add(_subtitle);

            _count = new Label
            {
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };
            _countBadge = new Border
            {
                Padding = new Thickness(10, 4),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 12 },
                VerticalOptions = LayoutOptions.Center,
                Content = _count,
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                ColumnSpacing = 12,
            };
            grid.Add(_iconChip, 0, 0);
            grid.Add(texts, 1, 0);
            grid.Add(_countBadge, 2, 0);

            View = new Border
            {
                Margin = new Thickness(16, 8, 16, 4),
                Padding = 14,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                Content = grid,
            };
        }

        public void Update(Trail t)
        {
            var accent = C(t.AccentColor);

            // Gradiente na cor de destaque do módulo (não no roxo do tema, que se confunde com o fundo).
            View.Background = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops =
                {
                    new GradientStop(accent, 0f),
                    new GradientStop(Lighten(accent, 0.18f), 1f),
                },
            };
            View.Stroke = (t.IsLocked ? C("#3d3870") : Lighten(accent, 0.3f)).WithAlpha(0.5f);
            View.Opacity = t.IsLocked ? 0.65 : 1.0;

            _iconChip.BackgroundColor = Colors.White.WithAlpha(0.2f);
            _icon.Text = t.IsLocked ? "🔒" : t.Emoji;
            _title.Text = t.Title;
            _subtitle.IsVisible = !string.IsNullOrWhiteSpace(t.Subtitle);
            _subtitle.Text = t.Subtitle ?? "";

            _countBadge.BackgroundColor = Colors.White.WithAlpha(0.2f);
            _count.Text = t.CountText;
        }

        /// <summary>Clareia uma cor misturando-a com branco (mantém o alfa).</summary>
        private static Color Lighten(Color c, float amt) => new(
            c.Red + (1 - c.Red) * amt,
            c.Green + (1 - c.Green) * amt,
            c.Blue + (1 - c.Blue) * amt,
            c.Alpha);
    }

    // ─── Nós e mascote (reconstruídos por trilha) ─────────────────────────────

    private View BuildNode(Trail t, TrailNode node)
    {
        var accent = C(t.AccentColor);
        double bubbleSize = node.IsBoss ? 72 : node.IsCurrent ? 68 : 60;

        var container = new VerticalStackLayout
        {
            Spacing = 6,
            WidthRequest = 100,
            HorizontalOptions = LayoutOptions.Center,
        };

        var nodeGroup = new VerticalStackLayout
        {
            Spacing = 6,
            HorizontalOptions = LayoutOptions.Center,
        };

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
            await bubble.TranslateTo(0, offsetY, 70, Easing.CubicIn);
            await bubble.TranslateTo(0, 0, 70, Easing.CubicOut);

            var cmd = NodeCommand;
            if (cmd?.CanExecute(node) == true)
                cmd.Execute(node);
        };
        holder.GestureRecognizers.Add(tap);
        nodeGroup.Add(holder);
        container.Add(nodeGroup);

        if (node.IsCurrent)
        {
            _floatTarget = nodeGroup;
            nodeGroup.Loaded += (_, _) => StartFloat(nodeGroup);
            nodeGroup.Unloaded += (_, _) =>
            {
                nodeGroup.AbortAnimation("nodeFloat");
                if (ReferenceEquals(_floatTarget, nodeGroup))
                    _floatTarget = null;
            };
        }

        // Nome da lição abaixo do nó — inclusive para lições bloqueadas (cor mais suave indica o cadeado).
        if (!string.IsNullOrWhiteSpace(node.Title))
        {
            container.Add(new Label
            {
                Text = node.Title,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = node.IsLocked ? C("#8a85b8") : C("#c4b8ff"),
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
