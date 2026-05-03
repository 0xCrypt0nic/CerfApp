namespace CessionApp.Services;

public interface ICerfaPageRenderer
{
    Task<List<ImageSource>> RenderPagesAsync(string pdfPath);
}
