using CerfApp.Models;
using CerfApp.Services;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace CerfApp;

public partial class SignaturePage : ContentPage
{
    private readonly CessionData _cession;
    private readonly bool _isVendeur;

    private readonly List<SKPath> _strokes = new();
    private SKPath? _currentPath;

    public SignaturePage(CessionData cession, bool isVendeur)
    {
        _cession  = cession;
        _isVendeur = isVendeur;
        InitializeComponent();

        Header.PageTitle        = isVendeur ? "Signature — vendeur" : "Signature — acheteur";
        InstructionLabel.Text   = isVendeur
            ? "Signature de l'ancien propriétaire"
            : "Signature du nouveau propriétaire";
    }

    // ── Dessin ────────────────────────────────────────────────────────────────

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                _currentPath = new SKPath();
                _currentPath.MoveTo(e.Location);
                _strokes.Add(_currentPath);
                break;

            case SKTouchAction.Moved:
                _currentPath?.LineTo(e.Location);
                CanvasView.InvalidateSurface();
                break;

            case SKTouchAction.Released:
                _currentPath = null;
                CanvasView.InvalidateSurface();
                break;
        }
        e.Handled = true;
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

        // Ligne de référence
        using var linePaint = new SKPaint
        {
            Color       = new SKColor(0xCC, 0xCC, 0xCC),
            StrokeWidth = 1,
            Style       = SKPaintStyle.Stroke,
            PathEffect  = SKPathEffect.CreateDash([8f, 6f], 0)
        };
        float lineY = e.Info.Height * 0.75f;
        canvas.DrawLine(20, lineY, e.Info.Width - 20, lineY, linePaint);

        // Traits de la signature
        using var strokePaint = new SKPaint
        {
            Color       = SKColors.Black,
            StrokeWidth = 3,
            Style       = SKPaintStyle.Stroke,
            StrokeJoin  = SKStrokeJoin.Round,
            StrokeCap   = SKStrokeCap.Round,
            IsAntialias = true
        };
        foreach (var path in _strokes)
            canvas.DrawPath(path, strokePaint);
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    private void OnEffacerClicked(object? sender, EventArgs e)
    {
        _strokes.Clear();
        CanvasView.InvalidateSurface();
    }

    private async void OnValiderClicked(object? sender, EventArgs e)
    {
        if (_strokes.Count == 0)
        {
            await DisplayAlertAsync("Signature manquante", "Veuillez signer avant de valider.", "OK");
            return;
        }

        var pngBytes = ExporterSignature();

        if (_isVendeur)
        {
            _cession.SignatureVendeur = pngBytes;
            await Navigation.PushAsync(new SignaturePage(_cession, isVendeur: false));
        }
        else
        {
            _cession.SignatureAcheteur = pngBytes;

            try
            {
                var pdfPath = await CerfaGenerator.GenererAsync(_cession);
                await Navigation.PushAsync(new PdfViewerPage(pdfPath));
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Erreur", $"Impossible de générer le PDF :\n{ex.Message}", "OK");
            }
        }
    }

    // ── Export PNG ────────────────────────────────────────────────────────────

    private byte[] ExporterSignature()
    {
        const int ExportW = 800;
        const int ExportH = 240;
        const int Padding = 20;

        // Calcule la bounding box de tous les traits
        var bounds = SKRect.Empty;
        foreach (var path in _strokes)
        {
            path.GetTightBounds(out var b);
            bounds = bounds.IsEmpty ? b : SKRect.Union(bounds, b);
        }

        var imageInfo = new SKImageInfo(ExportW, ExportH);
        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (!bounds.IsEmpty)
        {
            // Calcule le scale pour faire tenir la signature dans le rect d'export
            float availW = ExportW - Padding * 2;
            float availH = ExportH - Padding * 2;
            float scaleX = availW / bounds.Width;
            float scaleY = availH / bounds.Height;
            float scale  = Math.Min(scaleX, scaleY);

            float offsetX = Padding + (availW - bounds.Width  * scale) / 2f - bounds.Left * scale;
            float offsetY = Padding + (availH - bounds.Height * scale) / 2f - bounds.Top  * scale;

            canvas.Save();
            canvas.Translate(offsetX, offsetY);
            canvas.Scale(scale);

            using var paint = new SKPaint
            {
                Color       = SKColors.Black,
                StrokeWidth = 3f / scale,
                Style       = SKPaintStyle.Stroke,
                StrokeJoin  = SKStrokeJoin.Round,
                StrokeCap   = SKStrokeCap.Round,
                IsAntialias = true
            };
            foreach (var path in _strokes)
                canvas.DrawPath(path, paint);

            canvas.Restore();
        }

        using var image = surface.Snapshot();
        using var data  = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
