using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CerfApp.Services;

namespace CerfApp;

public partial class ScanPage : ContentPage
{
    private CancellationTokenSource? _cts;
    private bool _closing;
    private readonly CarteGriseData _data = new();
    private readonly bool _avecNumeroFormule;


    public CarteGriseData Result => _data;

    // Séparateur entre le code et la valeur : n'importe quel char non-alphanumérique (ex: ") ", ": ", " ")
    // ou un saut de ligne suivi de séparateurs optionnels
    private const string Sep = @"(?:[^\w\n]{0,8}|\s*\n[^\w\n]{0,4})";

    // Tous les champs sont sur la même ligne que leur label — pas de saut de ligne autorisé
    private const string SL = @"[^\w\n]{0,8}";

    // (A) — format SIV : AB-123-CD
    private static readonly Regex RxImmatSiv = new(
        @"\bA\b" + SL + @"([A-Z]{2}[-\s]?\d{3}[-\s]?[A-Z]{2})\b",
        RegexOptions.IgnoreCase | RegexOptions.Multiline);

    // (A) — format FNI : 123 AB 45
    private static readonly Regex RxImmatFni = new(
        @"\bA\b" + SL + @"(\d{1,4}\s*[A-Z]{2,3}\s*\d{2})\b",
        RegexOptions.IgnoreCase | RegexOptions.Multiline);

    // (I) — date du certificat (format FNI uniquement)
    private static readonly Regex RxDateCert = new(
        @"\bI\b" + SL + @"(\d{2}[./]\d{2}[./]\d{4})\b",
        RegexOptions.Multiline);

    // (E) — VIN 17 chars
    private static readonly Regex RxVin = new(
        @"\bE\b" + SL + @"([A-HJ-NPR-Z0-9]{17})\b",
        RegexOptions.Multiline);

    // (B) — date 1re immatriculation
    private static readonly Regex RxDate = new(
        @"\bB\b" + SL + @"(\d{2}[./]\d{2}[./]\d{4})\b",
        RegexOptions.Multiline);

    // (D.1)
    private static readonly Regex RxMarque = new(
        @"\bD[\s.]?1(?![\s.]*\d)\b" + SL + @"([A-Z][A-Z\-]{1,19}?)(?=\s|\n|$)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline);

    // (D.2)
    private static readonly Regex RxType = new(
        @"\bD[\s.]?2(?![\s.]*\d)\b" + SL + @"(.{3,40}?)(?=\s*\n|\s*$)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline);

    // (D.3)
    private static readonly Regex RxDenom = new(
        @"\bD[\s.]?3(?![\s.]*\d)\b" + SL + @"(.{2,30}?)(?=\s*\n|\s*$)",
        RegexOptions.IgnoreCase | RegexOptions.Multiline);

    // (J.1)
    private static readonly Regex RxGenre = new(
        @"\bJ[\s.]?1\b" + SL + @"([A-Z]{2,5})\b",
        RegexOptions.IgnoreCase | RegexOptions.Multiline);

    // Numéro de formule — ex: "20MA12345" ou "2015AF02648"
    private static readonly Regex RxNumFormule = new(
        @"\b([0-9]{2,4}[A-Z]{2}[0-9]{4,7})\b",
        RegexOptions.Multiline);

    public ScanPage(bool avecNumeroFormule = false)
    {
        _avecNumeroFormule = avecNumeroFormule;
        InitializeComponent();
        CameraView.Loaded += OnCameraViewLoaded;
    }

    private async void OnCameraViewLoaded(object? sender, EventArgs e)
    {
        CameraView.Loaded -= OnCameraViewLoaded;

        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlertAsync("Permission refusée", "L'accès à la caméra est nécessaire.", "OK");
            await Navigation.PopAsync();
            return;
        }

        await CameraView.StartCameraPreview(CancellationToken.None);

        _cts = new CancellationTokenSource();
        _ = BoucleCapturAsync(_cts.Token);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _cts?.Cancel();
        CameraView.StopCameraPreview();
    }

    private async Task BoucleCapturAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && !_data.IsComplete(_avecNumeroFormule))
        {
            try
            {
                await Task.Delay(800, ct);
                if (ct.IsCancellationRequested) break;
                await MainThread.InvokeOnMainThreadAsync(() => CameraView.CaptureImage(ct));
            }
            catch (OperationCanceledException) { break; }
            catch { }
        }
    }

    private async void OnMediaCaptured(object? sender, MediaCapturedEventArgs e)
    {
        if (_closing) return;
        try
        {
            using var ms = new MemoryStream();
            await e.Media.CopyToAsync(ms);
            var bytes = ms.ToArray();
            if (bytes.Length < 2000) return;

            var texte = await ReconnaitreTexteAsync(bytes);
            if (string.IsNullOrWhiteSpace(texte)) return;

            ExtractFields(texte);

            if (_data.IsComplete(_avecNumeroFormule))
                await CloseAsync();
        }
        catch { }
    }

    private void ExtractFields(string texte)
    {
        // Détection immat — essaie SIV puis FNI
        if (_data.Immatriculation == null)
        {
            var mSiv = RxImmatSiv.Match(texte);
            var mFni = !mSiv.Success ? RxImmatFni.Match(texte) : default;

            if (mSiv.Success || mFni.Success)
            {
                var raw = mSiv.Success ? mSiv.Groups[1].Value : mFni.Groups[1].Value;
                _data.EstAncienFormat = !mSiv.Success;
                _data.Immatriculation = raw.ToUpperInvariant().Replace(" ", "-");
                SetTrouve(IconA, TextA, "(A)", _data.Immatriculation);

                // Adapte le panneau secondaire selon le format détecté
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_data.EstAncienFormat)
                    {
                        NumFormuleRow.IsVisible = false;
                        DateCertRow.IsVisible   = true;
                    }
                    else if (_avecNumeroFormule)
                    {
                        NumFormuleRow.IsVisible = true;
                    }
                });
            }
        }

        Extraire(texte, RxVin,    _data.Vin == null,
            v => v.ToUpperInvariant(),
            v => { _data.Vin = v;             SetTrouve(IconE,  TextE,  "(E)",   v); });

        Extraire(texte, RxMarque, _data.Marque == null,
            v => v.Trim().ToUpperInvariant(),
            v => { _data.Marque = v;          SetTrouve(IconD1, TextD1, "(D.1)", v); });

        Extraire(texte, RxType,   _data.Type == null,
            v => v.Trim(),
            v => { _data.Type = v;            SetTrouve(IconD2, TextD2, "(D.2)", v); });

        Extraire(texte, RxDenom,  _data.Denomination == null,
            v => v.Trim(),
            v => { _data.Denomination = v;    SetTrouve(IconD3, TextD3, "(D.3)", v); });

        Extraire(texte, RxGenre,  _data.Genre == null,
            v => v.ToUpperInvariant(),
            v => { _data.Genre = v;           SetTrouve(IconJ1, TextJ1, "(J.1)", v); });

        Extraire(texte, RxDate,   _data.DateImmat == null,
            v => v,
            v => { _data.DateImmat = v;       SetTrouve(IconB,  TextB,  "(B)",   v); });

        if (_data.Immatriculation != null)
        {
            if (_data.EstAncienFormat)
                Extraire(texte, RxDateCert, _data.DateCertificat == null,
                    v => v,
                    v => { _data.DateCertificat = v; SetTrouve(IconI, TextI, "(I)", v); });
            else if (_avecNumeroFormule)
                Extraire(texte, RxNumFormule, _data.NumeroFormule == null,
                    v => v.ToUpperInvariant(),
                    v => { _data.NumeroFormule = v; SetTrouve(IconNF, TextNF, "N°", v); });
        }
    }

    private static void Extraire(string texte, Regex rx, bool champVide,
        Func<string, string> normalise, Action<string> accepte)
    {
        if (!champVide) return;
        var m = rx.Match(texte);
        if (m.Success)
            accepte(normalise(m.Groups[1].Value));
    }

    private void SetTrouve(Label icon, Label text, string label, string valeur)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            icon.Text = "✓";
            icon.TextColor = Color.FromArgb("#44DD44");
            text.Text = $"{label}  {valeur}";
            text.TextColor = Color.FromArgb("#44DD44");
        });
    }

    private async void OnTerminerClicked(object? sender, EventArgs e)
    {
        await CloseAsync();
    }

    private async Task CloseAsync()
    {
        if (_closing) return;
        _closing = true;
        _cts?.Cancel();
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Task.Delay(300);
            await Navigation.PopAsync();
        });
    }

    private Task<string?> ReconnaitreTexteAsync(byte[] bytes)
        => OcrService.ReconnaitreTexteAsync(bytes);
}
