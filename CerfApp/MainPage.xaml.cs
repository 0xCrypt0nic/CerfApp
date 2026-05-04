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

    private async void OnCerfa13750Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Cerfa13750TypePage(new Cerfa13750Data()));
    }
}
