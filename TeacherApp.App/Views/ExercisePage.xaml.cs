using TeacherApp.App.ViewModels;

namespace TeacherApp.App.Views;

public partial class ExercisePage : ContentPage
{
    public ExercisePage(ExerciseViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
