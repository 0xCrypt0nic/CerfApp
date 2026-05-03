using CerfApp.Models;

namespace CerfApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		var saved = Preferences.Get("app_theme", "dark");
		UserAppTheme = saved == "light" ? AppTheme.Light : AppTheme.Dark;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new NavigationPage(new MainPage()));
	}
}
