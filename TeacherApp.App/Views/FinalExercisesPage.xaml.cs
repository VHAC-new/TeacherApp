using TeacherApp.App.ViewModels;

namespace TeacherApp.App.Views;

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
}
