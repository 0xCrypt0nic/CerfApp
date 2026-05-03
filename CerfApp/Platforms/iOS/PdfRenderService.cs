using CoreGraphics;
using CerfApp.Services;
using PDFKit;
using UIKit;

namespace CerfApp;

public class PdfRenderService : ICerfaPageRenderer
{
    public Task<List<ImageSource>> RenderPagesAsync(string pdfPath)
    {
        var pages = new List<ImageSource>();

        var url      = Foundation.NSUrl.FromFilename(pdfPath);
        var document = new PDFDocument(url);
        if (document == null) return Task.FromResult(pages);

        for (nint i = 0; i < document.PageCount; i++)
        {
            var page = document.GetPage(i);
            if (page == null) continue;

            var bounds = page.Bounds;
            var scale  = 2.0f;
            var size   = new CGSize(bounds.Width * scale, bounds.Height * scale);

            UIGraphics.BeginImageContextWithOptions(size, true, 1.0f);
            var ctx = UIGraphics.GetCurrentContext()!;

            // Fond blanc
            ctx.SetFillColor(UIColor.White.CGColor);
            ctx.Fill(new CGRect(CGPoint.Empty, size));

            // Rendu de la page (axe Y inversé sur iOS)
            ctx.SaveState();
            ctx.TranslateCTM(0, size.Height);
            ctx.ScaleCTM(scale, -scale);
            page.Draw(ctx, bounds);
            ctx.RestoreState();

            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            if (image?.PNGRepresentation() is { } data)
            {
                var bytes = data.ToArray();
                pages.Add(ImageSource.FromStream(() => new MemoryStream(bytes)));
            }
        }

        return Task.FromResult(pages);
    }
}
