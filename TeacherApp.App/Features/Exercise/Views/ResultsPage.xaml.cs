using TeacherApp.App.Features.Exercise.ViewModels;

namespace TeacherApp.App.Features.Exercise.Views;

public partial class ResultsPage : ContentPage
{
    public ResultsPage(ResultsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
