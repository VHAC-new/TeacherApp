using TeacherApp.App.Views;

namespace TeacherApp.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("module", typeof(ModulePage));
        Routing.RegisterRoute("lesson", typeof(LessonPage));
        Routing.RegisterRoute("exercise", typeof(ExercisePage));
        Routing.RegisterRoute("final-exercises", typeof(FinalExercisesPage));
    }
}
