using TeacherApp.App.Core.Services;

namespace TeacherApp.App;

public partial class App : Application
{
	public App(AppThemeService themeService)
	{
		InitializeComponent();
		themeService.ApplySavedOrDefault(this);
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}