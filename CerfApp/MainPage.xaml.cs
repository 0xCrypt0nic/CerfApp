using CerfApp.Models;

namespace CerfApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnCerfa15776Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new VehiclePage(new CessionData()));
    }
}
