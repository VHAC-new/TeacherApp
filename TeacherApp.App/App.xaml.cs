using TeacherApp.App.Core.Services;

namespace TeacherApp.App;

public partial class App : Application
{
	private readonly AppSessionState _sessionState;

	public App(AppThemeService themeService, AppSessionState sessionState)
	{
		InitializeComponent();
		_sessionState = sessionState;
		themeService.ApplySavedOrDefault(this);
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
		// App retomado do background → pede revalidação one-shot (consumido pelas telas).
		window.Resumed += (_, _) => _sessionState.RequestResync();
		return window;
	}
}
