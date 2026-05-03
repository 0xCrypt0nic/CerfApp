using Android.Graphics;
using Android.Graphics.Pdf;
using CessionApp.Services;

namespace CessionApp;

public class PdfRenderService : ICerfaPageRenderer
{
    public Task<List<ImageSource>> RenderPagesAsync(string pdfPath)
    {
        var pages = new List<ImageSource>();

        using var fd = Android.OS.ParcelFileDescriptor.Open(
            new Java.IO.File(pdfPath),
            Android.OS.ParcelFileMode.ReadOnly)!;

        using var renderer = new PdfRenderer(fd);

        for (int i = 0; i < renderer.PageCount; i++)
        {
            using var page = renderer.OpenPage(i)!;

            // Rendu à 2× pour une bonne lisibilité sur écran
            int width  = page.Width  * 2;
            int height = page.Height * 2;

            using var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888!)!;
            bitmap.EraseColor(Android.Graphics.Color.White);
            page.Render(bitmap, null, null, PdfRenderMode.ForDisplay);

            using var ms = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Png!, 95, ms);
            var bytes = ms.ToArray();

            pages.Add(ImageSource.FromStream(() => new MemoryStream(bytes)));
        }

        return Task.FromResult(pages);
    }
}
