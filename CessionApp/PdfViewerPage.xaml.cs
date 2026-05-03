using CessionApp.Services;

namespace CessionApp;

public partial class PdfViewerPage : ContentPage
{
    private readonly string            _pdfPath;
    private readonly ICerfaPageRenderer _renderer;

    public PdfViewerPage(string pdfPath)
    {
        _pdfPath  = pdfPath;
        _renderer = Application.Current!.Handler!.MauiContext!
                    .Services.GetRequiredService<ICerfaPageRenderer>();
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var pages = await _renderer.RenderPagesAsync(_pdfPath);

            foreach (var pageSource in pages)
            {
                PagesContainer.Children.Add(new Image
                {
                    Source            = pageSource,
                    HorizontalOptions = LayoutOptions.Fill,
                    Aspect            = Aspect.AspectFit
                });
            }

            Loader.IsVisible    = false;
            Loader.IsRunning    = false;
            PdfScrollView.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erreur", $"Impossible d'afficher le PDF :\n{ex.Message}", "OK");
        }
    }

    private async void OnPartagerClicked(object? sender, EventArgs e)
    {
        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Certificat de cession",
            File  = new ShareFile(_pdfPath)
        });
    }
}
