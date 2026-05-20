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
}
