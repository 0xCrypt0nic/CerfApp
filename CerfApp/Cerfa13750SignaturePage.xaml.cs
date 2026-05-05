using CerfApp.Models;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace CerfApp;

public partial class Cerfa13750SignaturePage : ContentPage
{
    private enum Partie { Titulaire, Loueur, Locataire }

    private readonly Cerfa13750Data _data;
    private readonly Cerfa13750RecapPage _recap;
    private Partie _partie;

    private readonly List<SKPath> _strokes = new();
    private SKPath? _currentPath;

    public Cerfa13750SignaturePage(Cerfa13750Data data)
    {
        InitializeComponent();
        _data = data;

        // Retrouver la RecapPage dans la pile de navigation
        _recap = Navigation.NavigationStack
            .OfType<Cerfa13750RecapPage>()
            .Last();

        SetPartie(Partie.Titulaire);
    }

    private void SetPartie(Partie partie)
    {
        _partie = partie;
        _strokes.Clear();
        CanvasView?.InvalidateSurface();

        (Header.PageTitle, InstructionLabel.Text, ValiderButton.Text) = partie switch
        {
            Partie.Titulaire => ("Signature — Titulaire",  "Signature du titulaire",  SuivantLabel()),
            Partie.Loueur    => ("Signature — Loueur",     "Signature du loueur",     SuivantLabel()),
            Partie.Locataire => ("Signature — Locataire",  "Signature du locataire",  "Valider"),
            _                => ("", "", "")
        };
    }

    private string SuivantLabel()
    {
        var next = ProchainPartie();
        return next.HasValue ? "Suivant" : "Valider";
    }

    private Partie? ProchainPartie() => _partie switch
    {
        Partie.Titulaire when _data.SituationLocative != SituationLocative.Non
            => Partie.Loueur,
        Partie.Loueur when _data.SituationLocative == SituationLocative.LongueDuree ||
                           _data.SituationLocative == SituationLocative.CreditBail
            => Partie.Locataire,
        _ => null
    };

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

        using var linePaint = new SKPaint
        {
            Color       = new SKColor(0xCC, 0xCC, 0xCC),
            StrokeWidth = 1,
            Style       = SKPaintStyle.Stroke,
            PathEffect  = SKPathEffect.CreateDash([8f, 6f], 0)
        };
        float lineY = e.Info.Height * 0.75f;
        canvas.DrawLine(20, lineY, e.Info.Width - 20, lineY, linePaint);

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
            await DisplayAlert("Signature manquante", "Veuillez signer avant de valider.", "OK");
            return;
        }

        var png = ExporterSignature();

        switch (_partie)
        {
            case Partie.Titulaire: _data.SignatureTitulaire = png; break;
            case Partie.Loueur:    _data.SignatureLoueur    = png; break;
            case Partie.Locataire: _data.SignatureLocataire = png; break;
        }

        var next = ProchainPartie();
        if (next.HasValue)
        {
            SetPartie(next.Value);
        }
        else
        {
            await _recap.DemanderOppositionEtGenerer();
        }
    }

    // ── Export PNG ────────────────────────────────────────────────────────────

    private byte[] ExporterSignature()
    {
        const int W = 800, H = 240, Pad = 20;

        var bounds = SKRect.Empty;
        foreach (var path in _strokes)
        {
            path.GetTightBounds(out var b);
            bounds = bounds.IsEmpty ? b : SKRect.Union(bounds, b);
        }

        using var surface = SKSurface.Create(new SKImageInfo(W, H));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (!bounds.IsEmpty)
        {
            float availW = W - Pad * 2, availH = H - Pad * 2;
            float scale  = Math.Min(availW / bounds.Width, availH / bounds.Height);
            float offX   = Pad + (availW - bounds.Width  * scale) / 2f - bounds.Left * scale;
            float offY   = Pad + (availH - bounds.Height * scale) / 2f - bounds.Top  * scale;

            canvas.Save();
            canvas.Translate(offX, offY);
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
