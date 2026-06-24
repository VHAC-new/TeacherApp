using System.ComponentModel;
using TeacherApp.App.Core;
using TeacherApp.App.Features.Home.ViewModels;

namespace TeacherApp.App.Features.Home.Views;

public partial class LessonsPage : ContentPage
{
    private const string MascotAnimation = "mascotBounce";
    private readonly LessonsViewModel _vm;

    public LessonsPage(LessonsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.PropertyChanged += OnViewModelPropertyChanged;
        if (_vm.IsLoading)
            StartMascotAnimation();

        _ = _vm.InitializeCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        _vm.PropertyChanged -= OnViewModelPropertyChanged;
        StopMascotAnimation();

        if (BindingContext is ICleanup cleanup)
            cleanup.Cleanup();

        base.OnDisappearing();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(LessonsViewModel.IsLoading))
            return;

        if (_vm.IsLoading)
            StartMascotAnimation();
        else
            StopMascotAnimation();
    }

    // Float + pulse contínuos do lobo enquanto a trilha carrega (animação nativa, sem Lottie).
    private void StartMascotAnimation()
    {
        var anim = new Animation
        {
            { 0.0, 0.5, new Animation(v => LoadingMascot.TranslationY = v, 0, -16, Easing.SinInOut) },
            { 0.5, 1.0, new Animation(v => LoadingMascot.TranslationY = v, -16, 0, Easing.SinInOut) },
            { 0.0, 0.5, new Animation(v => LoadingMascot.Scale = v, 1.0, 1.08, Easing.SinInOut) },
            { 0.5, 1.0, new Animation(v => LoadingMascot.Scale = v, 1.08, 1.0, Easing.SinInOut) },
        };
        LoadingMascot.Animate(MascotAnimation, anim, length: 1400, repeat: () => true);
    }

    private void StopMascotAnimation()
    {
        LoadingMascot.AbortAnimation(MascotAnimation);
        LoadingMascot.TranslationY = 0;
        LoadingMascot.Scale = 1.0;
    }
}
