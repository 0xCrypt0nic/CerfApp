namespace CerfApp;

public partial class ParametresPage : ContentPage
{
    public ParametresPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var saved = Preferences.Get("app_theme", "dark");
        ModeSombreRadio.IsChecked = saved == "dark";
        ModeCLairRadio.IsChecked  = saved == "light";
    }

    private void OnThemeChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;

        if (sender == ModeSombreRadio)
        {
            Application.Current!.UserAppTheme = AppTheme.Dark;
            Preferences.Set("app_theme", "dark");
        }
        else
        {
            Application.Current!.UserAppTheme = AppTheme.Light;
            Preferences.Set("app_theme", "light");
        }
    }
}
