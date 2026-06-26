using System.ComponentModel;
using TeacherApp.App.Core;
using TeacherApp.App.Features.Exercise.ViewModels;

namespace TeacherApp.App.Features.Exercise.Views;

public partial class ExercisePage : ContentPage
{
    private readonly ExerciseViewModel _vm;
    private bool _hintAnimating;
    private bool _pageActive = true;
    private bool _floating;

    public ExercisePage(ExerciseViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _vm.PropertyChanged += OnViewModelPropertyChanged;
        _vm.BeforeNavigateToResults = PlayCompletionTransitionAsync;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _pageActive = true;
        StartFloating();
        await _vm.LoadCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        _pageActive = false;
        _floating = false;
        AbortAnimations();
        _vm.PropertyChanged -= OnViewModelPropertyChanged;
        _vm.BeforeNavigateToResults = null;

        if (BindingContext is ICleanup cleanup)
            cleanup.Cleanup();

        base.OnDisappearing();
    }

    private void AbortAnimations()
    {
        HintContent.AbortAnimation("HintExpand");
        HintContent.AbortAnimation("HintCollapse");
        PageRoot.AbortAnimation("CompletionExit");
        Mascot.AbortAnimation("MascotFloat");
    }

    // Float contínuo do avatar (padrão reutilizado da LessonsPage).
    private void StartFloating()
    {
        if (_floating)
            return;

        _floating = true;
        var anim = new Animation
        {
            { 0, 1, new Animation(v => Mascot.TranslationY = -10 * Math.Sin(v * Math.PI)) },
            { 0, 1, new Animation(v => Mascot.Scale = 1 + 0.04 * Math.Sin(v * Math.PI)) },
        };
        Mascot.Animate("MascotFloat", anim, length: 1600, repeat: () => _floating);
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_pageActive)
            return;

        switch (e.PropertyName)
        {
            case nameof(ExerciseViewModel.IsHintVisible):
                await HandleHintAsync();
                break;

            case nameof(ExerciseViewModel.LastResult):
                await HandleResultAsync();
                break;
        }
    }

    private async Task HandleHintAsync()
    {
        if (_hintAnimating)
            return;

        _hintAnimating = true;
        try
        {
            if (_vm.IsHintVisible)
                await ExpandHintAsync();
            else
                await CollapseHintAsync();
        }
        finally
        {
            _hintAnimating = false;
        }
    }

    private async Task HandleResultAsync()
    {
        // Próxima questão: esconde o balão e reposiciona o avatar.
        if (_vm.LastResult is null)
        {
            Mascot.AbortAnimation("MascotFloat");
            Mascot.TranslationX = 0;
            Mascot.Scale = 1;
            StartFloating();
            SpeechBubble.IsVisible = false;
            SpeechBubble.Opacity = 0;
            return;
        }

        await RevealBubbleAsync();

        if (_vm.LastResult == true)
        {
            await Task.WhenAll(
                Mascot.BounceAsync(),
                Confetti.BurstAsync());
        }
        else
        {
            await Mascot.ShakeAsync();
        }
    }

    private async Task RevealBubbleAsync()
    {
        if (!_pageActive)
            return;

        SpeechBubble.Opacity = 0;
        SpeechBubble.Scale = 0.85;
        SpeechBubble.IsVisible = true;
        await Task.WhenAll(
            SpeechBubble.FadeTo(1, 220, Easing.CubicOut),
            SpeechBubble.ScaleTo(1, 260, Easing.SpringOut));
    }

    private async Task ExpandHintAsync()
    {
        if (!_pageActive)
            return;

        HintContent.AbortAnimation("HintExpand");
        HintContent.AbortAnimation("HintCollapse");
        HintContent.IsVisible = true;
        HintContent.Opacity = 0;
        HintContent.TranslationY = -10;

        await Task.WhenAll(
            HintContent.FadeTo(1, 800, Easing.CubicOut),
            HintContent.TranslateTo(0, 0, 800, Easing.CubicOut));

        if (!_pageActive)
            AbortAnimations();
    }

    private async Task CollapseHintAsync()
    {
        if (!_pageActive || !HintContent.IsVisible)
            return;

        HintContent.AbortAnimation("HintExpand");
        HintContent.AbortAnimation("HintCollapse");

        await Task.WhenAll(
            HintContent.FadeTo(0, 220, Easing.CubicIn),
            HintContent.TranslateTo(0, -10, 220, Easing.CubicIn));

        if (!_pageActive)
            return;

        HintContent.IsVisible = false;
        HintContent.Opacity = 1;
        HintContent.TranslationY = 0;
    }

    private async Task PlayCompletionTransitionAsync()
    {
        if (!_pageActive)
            return;

        PageRoot.AbortAnimation("CompletionExit");

        var overlay = new BoxView
        {
            Color = Color.FromArgb("#7C5DF7"),
            Opacity = 0,
            InputTransparent = true,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        if (PageRoot is Grid grid)
        {
            Grid.SetRowSpan(overlay, grid.RowDefinitions.Count > 0 ? grid.RowDefinitions.Count : 1);
            grid.Add(overlay);
        }

        await Task.WhenAll(
            PageRoot.FadeTo(0, 320, Easing.CubicIn),
            PageRoot.ScaleTo(0.94, 320, Easing.CubicIn),
            overlay.FadeTo(0.35, 320, Easing.CubicIn));

        if (PageRoot is Grid g && _pageActive)
            g.Remove(overlay);
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
