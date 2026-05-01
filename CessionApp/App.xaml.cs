using CessionApp.Models;

namespace CessionApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var cession = new CessionData();
		return new Window(new NavigationPage(new MainPage(cession)));
	}
}