using TeacherApp.App.Core;
using TeacherApp.App.Features.Exercise.ViewModels;

namespace TeacherApp.App.Features.Exercise.Views;

public partial class FinalExercisesPage : ContentPage
{
    private readonly FinalExercisesViewModel _vm;

    public FinalExercisesPage(FinalExercisesViewModel vm)
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
}
