using TeacherApp.App.Core;
using TeacherApp.App.Features.Exercise.ViewModels;

namespace TeacherApp.App.Features.Exercise.Views;

public partial class ExercisePage : ContentPage
{
    private readonly ExerciseViewModel _vm;

    public ExercisePage(ExerciseViewModel vm)
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
        if (BindingContext is ICleanup cleanup)
            cleanup.Cleanup();
        base.OnDisappearing();
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
