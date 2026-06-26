using System.ComponentModel;
using TeacherApp.App.Core;
using TeacherApp.App.Features.Home.ViewModels;

namespace TeacherApp.App.Features.Home.Views;

public partial class LessonsPage : ContentPage
{
    private readonly LessonsViewModel _vm;
    private bool _animating;

    public LessonsPage(LessonsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        _vm.PropertyChanged += OnVmPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateLoadingAnimation();
        await _vm.InitializeCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is ICleanup cleanup)
            cleanup.Cleanup();

        base.OnDisappearing();
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(LessonsViewModel.IsLoading) or nameof(LessonsViewModel.State))
            UpdateLoadingAnimation();
    }

    private void UpdateLoadingAnimation()
    {
        if (_vm.IsLoading && !_animating)
        {
            _animating = true;
            AnimateMascot();
        }
        else if (!_vm.IsLoading)
        {
            _animating = false;
            LoadingMascot.AbortAnimation("loadFloat");
        }
    }

    private void AnimateMascot()
    {
        var anim = new Animation
        {
            { 0, 1, new Animation(v => LoadingMascot.TranslationY = -10 * Math.Sin(v * Math.PI)) },
            { 0, 1, new Animation(v => LoadingMascot.Scale = 1 + 0.05 * Math.Sin(v * Math.PI)) },
        };
        LoadingMascot.Animate("loadFloat", anim, length: 1400, repeat: () => _animating);
    }
}
