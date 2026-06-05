using TeacherApp.App.Features.Exercise.Views;
using TeacherApp.App.Features.Lesson.Views;
using TeacherApp.App.Features.Module.Views;

namespace TeacherApp.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("module", typeof(ModulePage));
        Routing.RegisterRoute("lesson", typeof(LessonPage));
        Routing.RegisterRoute("exercise", typeof(ExercisePage));
        Routing.RegisterRoute("results", typeof(ResultsPage));
        Routing.RegisterRoute("final-exercises", typeof(FinalExercisesPage));
    }
}
