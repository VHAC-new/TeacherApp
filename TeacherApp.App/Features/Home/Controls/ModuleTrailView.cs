using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using TeacherApp.App.Features.Home.Models;

namespace TeacherApp.App.Features.Home.Controls;

/// <summary>
/// Renderiza um <see cref="TrailModule"/> (módulo/"oceano"): card de cabeçalho com progresso
/// agregado + uma pilha de <see cref="TrailView"/> (uma por trilha). É o item do CollectionView
/// da tela de aulas. Dispara o carregamento (lazy) das trilhas quando o módulo entra na viewport.
/// </summary>
public sealed class ModuleTrailView : ContentView
{
    public static readonly BindableProperty NodeCommandProperty =
        BindableProperty.Create(nameof(NodeCommand), typeof(ICommand), typeof(ModuleTrailView));

    public ICommand? NodeCommand
    {
        get => (ICommand?)GetValue(NodeCommandProperty);
        set => SetValue(NodeCommandProperty, value);
    }

    /// <summary>Comando que dispara o carregamento (lazy) das trilhas do módulo.</summary>
    public static readonly BindableProperty EnsureLoadedCommandProperty =
        BindableProperty.Create(nameof(EnsureLoadedCommand), typeof(ICommand), typeof(ModuleTrailView));

    public ICommand? EnsureLoadedCommand
    {
        get => (ICommand?)GetValue(EnsureLoadedCommandProperty);
        set => SetValue(EnsureLoadedCommandProperty, value);
    }

    private TrailModule? _module;

    private readonly HeaderCard _header;
    private readonly VerticalStackLayout _trailsHost;
    private readonly VerticalStackLayout _root;

    public ModuleTrailView()
    {
        _header = new HeaderCard();
        _trailsHost = new VerticalStackLayout { Spacing = 4 };

        _root = new VerticalStackLayout { Spacing = 0, Margin = new Thickness(0, 8, 0, 16) };
        _root.Add(_header.View);
        _root.Add(_trailsHost);

        Content = _root;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();

        if (_module is not null)
            _module.PropertyChanged -= OnModulePropertyChanged;

        _module = BindingContext as TrailModule;

        if (_module is not null)
            _module.PropertyChanged += OnModulePropertyChanged;

        Rebuild();
        TriggerLoadIfNeeded();
    }

    private void OnModulePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        bool relevant = e.PropertyName is nameof(TrailModule.LoadState)
            or nameof(TrailModule.IsLocked)
            or nameof(TrailModule.Trails)
            or nameof(TrailModule.CompletedCount)
            or nameof(TrailModule.TotalCount);

        if (!relevant)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Rebuild();
            TriggerLoadIfNeeded();
        });
    }

    private void TriggerLoadIfNeeded()
    {
        // Módulos bloqueados também carregam: exibimos a trilha/aulas com cadeado.
        if (_module is { LoadState: TrailModuleLoadState.NotLoaded } m
            && EnsureLoadedCommand?.CanExecute(m) == true)
        {
            EnsureLoadedCommand.Execute(m);
        }
    }

    private void Rebuild()
    {
        if (_module is null)
            return;

        _header.Update(_module);
        RebuildTrails(_module);
    }

    private void RebuildTrails(TrailModule m)
    {
        _trailsHost.Children.Clear();

        if (m.LoadState == TrailModuleLoadState.Error)
        {
            _trailsHost.Add(BuildErrorView(m));
            return;
        }

        if (m.LoadState != TrailModuleLoadState.Loaded)
        {
            _trailsHost.Add(new ActivityIndicator
            {
                IsRunning = true,
                Color = Color.FromArgb(m.AccentColor),
                WidthRequest = 28,
                HeightRequest = 28,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 24, 0, 24),
            });
            return;
        }

        if (m.Trails.Count == 0)
        {
            _trailsHost.Add(BuildMessage("Nenhuma trilha disponível neste módulo."));
            return;
        }

        foreach (var trail in m.Trails)
        {
            var view = new TrailView { BindingContext = trail };
            view.SetBinding(TrailView.NodeCommandProperty, new Binding(nameof(NodeCommand), source: this));
            _trailsHost.Add(view);
        }
    }

    private View BuildErrorView(TrailModule m)
    {
        var message = new Label
        {
            Text = string.IsNullOrWhiteSpace(m.LoadError)
                ? "Não foi possível carregar as trilhas."
                : $"Erro ao carregar: {m.LoadError}",
            FontSize = 13,
            TextColor = Color.FromArgb("#f87171"),
            HorizontalTextAlignment = TextAlignment.Center,
        };

        var retry = new Label
        {
            Text = "Toque para tentar novamente",
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#c4b8ff"),
            HorizontalTextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 8, 0, 0),
        };

        var content = new VerticalStackLayout { Spacing = 0 };
        content.Add(message);
        content.Add(retry);

        var border = new Border
        {
            Margin = new Thickness(16, 8, 16, 4),
            Padding = 16,
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#3d3870"),
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Content = content,
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            // Volta a NotLoaded: a cascata de PropertyChanged dispara o recarregamento.
            if (_module is not null)
                _module.LoadState = TrailModuleLoadState.NotLoaded;
        };
        border.GestureRecognizers.Add(tap);

        return border;
    }

    private static View BuildMessage(string text) => new Border
    {
        Margin = new Thickness(16, 8, 16, 4),
        Padding = 16,
        StrokeThickness = 1,
        Stroke = Color.FromArgb("#3d3870"),
        StrokeShape = new RoundRectangle { CornerRadius = 14 },
        Content = new Label
        {
            Text = text,
            FontSize = 13,
            TextColor = Color.FromArgb("#8a85b8"),
            HorizontalTextAlignment = TextAlignment.Center,
        },
    };

    // ─── Cabeçalho do módulo (card grande "OCEANO N") ─────────────────────────

    private sealed class HeaderCard
    {
        public Border View { get; }

        private readonly Border _emojiChip;
        private readonly Label _emoji;
        private readonly Label _headerLabel;
        private readonly Label _title;
        private readonly Label _subtitle;
        private readonly Label _count;
        private readonly ProgressBar _progress;

        public HeaderCard()
        {
            _emoji = new Label { FontSize = 20, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            _emojiChip = new Border
            {
                WidthRequest = 44,
                HeightRequest = 44,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Content = _emoji,
            };

            _headerLabel = new Label
            {
                FontSize = 11,
                FontAttributes = FontAttributes.Bold,
                TextColor = C("#c4b5fd"),
                CharacterSpacing = 1.5,
            };
            _title = new Label
            {
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
            };
            var titleStack = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
            titleStack.Add(_headerLabel);
            titleStack.Add(_title);

            var row1 = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star),
                },
                ColumnSpacing = 12,
            };
            row1.Add(_emojiChip, 0, 0);
            row1.Add(titleStack, 1, 0);

            _subtitle = new Label
            {
                FontSize = 13,
                TextColor = C("#c4b8ff"),
                Margin = new Thickness(0, 8, 0, 0),
            };

            _progress = new ProgressBar
            {
                HeightRequest = 8,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center,
            };
            _count = new Label
            {
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                VerticalOptions = LayoutOptions.Center,
            };
            var progressRow = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto),
                },
                ColumnSpacing = 10,
                Margin = new Thickness(0, 10, 0, 0),
            };
            progressRow.Add(_progress, 0, 0);
            progressRow.Add(_count, 1, 0);

            var inner = new VerticalStackLayout { Spacing = 0 };
            inner.Add(row1);
            inner.Add(_subtitle);
            inner.Add(progressRow);

            // Cabeçalho do módulo é "clean": texto direto sobre o fundo, sem card/gradiente/borda.
            // Apenas as trilhas (abaixo) são cards.
            View = new Border
            {
                Margin = new Thickness(20, 4, 20, 10),
                Padding = 0,
                BackgroundColor = Colors.Transparent,
                StrokeThickness = 0,
                Content = inner,
            };
        }

        public void Update(TrailModule m)
        {
            _emojiChip.BackgroundColor = Colors.White.WithAlpha(0.12f);
            _emoji.Text = m.Emoji;
            _headerLabel.Text = m.HeaderLabel;
            _title.Text = m.Title;

            _subtitle.IsVisible = !string.IsNullOrWhiteSpace(m.Subtitle);
            _subtitle.Text = m.Subtitle ?? "";

            _progress.Progress = m.ProgressFraction;
            _progress.ProgressColor = m.IsLocked ? C("#5a5490") : Colors.White;
            _count.Text = m.IsLocked ? "🔒" : m.CountText;
        }

        private static Color C(string hex) => Color.FromArgb(hex);
    }
}
