namespace CerfApp;

public partial class ParametresPage : ContentPage
{
    private bool _initializing;

    public ParametresPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _initializing = true;
        var saved = Preferences.Get("app_theme", "dark");
        ModeSombreRadio.IsChecked = saved == "dark";
        ModeCLairRadio.IsChecked  = saved == "light";
        _initializing = false;
    }

    private void OnThemeChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!e.Value || _initializing) return;

        var target = sender == ModeSombreRadio ? AppTheme.Dark : AppTheme.Light;

        // Ne rien faire si le thème est déjà le bon
        if (Application.Current!.UserAppTheme == target) return;

        Application.Current.UserAppTheme = target;
        Preferences.Set("app_theme", target == AppTheme.Dark ? "dark" : "light");
    }
}
