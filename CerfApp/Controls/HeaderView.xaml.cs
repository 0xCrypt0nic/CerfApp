namespace CerfApp.Controls;

public partial class HeaderView : ContentView
{
    public static readonly BindableProperty PageTitleProperty =
        BindableProperty.Create(nameof(PageTitle), typeof(string), typeof(HeaderView),
            "Certificat de cession (15776*02)");

    public static readonly BindableProperty ShowBackProperty =
        BindableProperty.Create(nameof(ShowBack), typeof(bool), typeof(HeaderView), true);

    public string PageTitle
    {
        get => (string)GetValue(PageTitleProperty);
        set => SetValue(PageTitleProperty, value);
    }

    public bool ShowBack
    {
        get => (bool)GetValue(ShowBackProperty);
        set => SetValue(ShowBackProperty, value);
    }

    public HeaderView()
    {
        InitializeComponent();
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
