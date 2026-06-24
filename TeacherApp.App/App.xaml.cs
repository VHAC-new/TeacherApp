using TeacherApp.App.Core.Services;

namespace TeacherApp.App;

public partial class App : Application
{
	private readonly AppSessionState _appSession;

	public App(AppThemeService themeService, AppSessionState appSession)
	{
		InitializeComponent();
		_appSession = appSession;
		themeService.ApplySavedOrDefault(this);
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
		// App voltando do background → pede uma revalidação silenciosa na próxima tela.
		window.Resumed += (_, _) => _appSession.RequestResync();
		return window;
	}
}