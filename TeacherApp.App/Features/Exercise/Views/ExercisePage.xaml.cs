using System.ComponentModel;
using TeacherApp.App.Core;
using TeacherApp.App.Features.Exercise.ViewModels;

namespace TeacherApp.App.Features.Exercise.Views;

public partial class ExercisePage : ContentPage
{
    private readonly ExerciseViewModel _vm;
    private bool _hintAnimating;
    private bool _pageActive = true;

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
        await _vm.LoadCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        _pageActive = false;
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
    }

    private async void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_pageActive || e.PropertyName != nameof(ExerciseViewModel.IsHintVisible) || _hintAnimating)
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
            Color = Color.FromArgb("#4F7CFF"),
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
