using TeacherApp.App.Core;
using TeacherApp.App.Features.Lesson.ViewModels;

namespace TeacherApp.App.Features.Lesson.Views;

public partial class LessonPage : ContentPage
{
    private readonly LessonViewModel _vm;

    public LessonPage(LessonViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }

    protected override void OnDisappearing()
    {
        _vm.StopAudio();

        if (BindingContext is ICleanup cleanup)
            cleanup.Cleanup();
        base.OnDisappearing();
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
