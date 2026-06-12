using TeacherApp.App.Features.Exercise.Views;
using TeacherApp.App.Features.Lesson.Views;
using TeacherApp.App.Features.Module.Views;
using TeacherApp.App.Features.Profile.Views;
#if ANDROID
using TeacherApp.App.Platforms.Android;
#elif IOS
using TeacherApp.App.Platforms.iOS;
#endif

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
        Routing.RegisterRoute("change-password", typeof(ChangePasswordPage));

        Navigated += OnShellNavigated;
    }

    private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        ApplyTabBarBorder();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        ApplyTabBarBorder();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyTabBarBorder();
    }

    private void ApplyTabBarBorder()
    {
#if ANDROID
        TabBarBorderHelper.ApplyToShell(Handler?.PlatformView);
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
            TabBarBorderHelper.ApplyToShell(Handler?.PlatformView));
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(400), () =>
            TabBarBorderHelper.ApplyToShell(Handler?.PlatformView));
#elif IOS
        TabBarBorderHelper.ApplyToShell(Handler?.PlatformView);
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
            TabBarBorderHelper.ApplyToShell(Handler?.PlatformView));
#endif
    }
}
