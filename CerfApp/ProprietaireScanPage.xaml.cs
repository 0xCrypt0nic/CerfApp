using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CerfApp.Models;
using CerfApp.Services;

namespace CerfApp;

public partial class ProprietaireScanPage : ContentPage
{
    private CancellationTokenSource? _cts;
    private bool _closing;
    private readonly ProprietaireOcrData _data = new();
    private readonly bool _avecNaissance;

    public ProprietaireOcrData Result => _data;

    // ── Carte grise ───────────────────────────────────────────────────────────
    // Nom : valeur sur la même ligne que C.1
    private static readonly Regex RxCgNom = new(
        @"\bC[^a-zA-Z0-9\n]?1(?!\d)[^\w\n]{0,10}([A-ZÀÂÉÈÊËÎÏÔÙÛÜÇ][A-ZÀÂÉÈÊËÎÏÔÙÛÜÇ\s\-]{0,30}?)\s*(?:\n|$)",
        RegexOptions.Multiline);

    // Prénom cas 1 : ligne juste après C.1 (ex: "C.1 DUPONT\nYVES")
    private static readonly Regex RxCgPrenomApresC1 = new(
        @"\bC[^a-zA-Z0-9\n]?1(?!\d)[^\n]*\n[^\w\n]{0,15}([A-ZÀÂÉÈÊËÎÏÔÙÛÜÇ][A-ZÀ-Ÿa-zà-ÿ\-]{1,25}?)\s*(?:\n|$)",
        RegexOptions.Multiline);

    // Prénom cas 2 : ligne juste après C4.1 (prénom sur la ligne suivante)
    private static readonly Regex RxCgPrenomApresC41 = new(
        @"\bC[^a-zA-Z0-9\n]?4[^a-zA-Z0-9\n]?1(?!\d)[^\n]*\n[^\w\n]{0,15}([A-ZÀÂÉÈÊËÎÏÔÙÛÜÇ][A-ZÀ-Ÿa-zà-ÿ\-]{1,25}?)\s*(?:\n|$)",
        RegexOptions.Multiline);

    // Adresse : numéro + type de voie, indépendant du reste
    private static readonly Regex RxCgAdresse = new(
        @"(\d{1,4}\s+(?:RUE|AVENUE|BOULEVARD|BD|CHEMIN|IMPASSE|ALL[EÉ]E|VOIE|ROUTE|PLACE|SQUARE|R[EÉ]SIDENCE|PASSAGE|SENTIER|MONT[EÉ]E|LOTISSEMENT|LOT|DOMAINE|HAMEAU|VILLA|CIT[EÉ])[^\n]{0,50}?)\s*(?:\n|$)",
        RegexOptions.Multiline | RegexOptions.IgnoreCase);

    // Code postal + commune : indépendant
    private static readonly Regex RxCgCPCommune = new(
        @"\b((?:0[1-9]|[1-8]\d|9[0-5])\d{3})\s+([A-ZÀÂÉÈÊËÎÏÔÙÛÜÇ][A-ZÀÂÉÈÊËÎÏÔÙÛÜÇ\s\-]{1,40}?)\s*(?:\n|$)",
        RegexOptions.Multiline);

    // ── CNI ────────────────────────────────────────────────────────────────────
    private static readonly Regex RxCniNom = new(
        @"\bNOM\b[^\w\n]{0,8}([A-ZÀÂÉÈÊËÎÏÔÙÛÜÇ][A-ZÀÂÉÈÊËÎÏÔÙÛÜÇ\s\-]{1,30}?)\s*(?:\n|$)",
        RegexOptions.Multiline);

    private static readonly Regex RxCniPrenom = new(
        @"\bPR[EÉ]NOMS?\b[^\w\n]{0,8}([A-ZÀ-Ÿa-zà-ÿ][A-ZÀ-Ÿa-zà-ÿ\-]{1,30}?)(?=[,\s]*\n|\s*$)",
        RegexOptions.Multiline | RegexOptions.IgnoreCase);

    // Adresse CNI : première ligne non-vide après le label ADRESSE
    private static readonly Regex RxCniAdresse = new(
        @"\bADRESSE\b[^\n]*\n[^\w\n]{0,5}([^\n]{5,60}?)\s*(?:\n|$)",
        RegexOptions.Multiline | RegexOptions.IgnoreCase);

    // Date de naissance CNI — label souple, date sur la même ligne OU la suivante
    private static readonly Regex RxCniDateNaissance = new(
        @"(?:DATE\s+DE\s+NAISS|N[EÉ]E?\s+LE)[^\n]{0,40}?\n?[^\w\n]{0,10}(\d{1,2}[\s./]\d{2}[\s./]\d{4})",
        RegexOptions.Multiline | RegexOptions.IgnoreCase);

    // Lieu de naissance CNI nouvelle (label LIEU DE NAISSANCE), ville sur la même ligne ou la suivante
    private static readonly Regex RxCniLieuNaissance = new(
        @"LIEU\s+DE\s+NAISSANCE[^\n]{0,40}\n?[^\w\n]{0,10}([A-ZÀÂÉÈÊËÎÏÔÙÛÜÇ][A-ZÀÂÉÈÊËÎÏÔÙÛÜÇ\s\-0-9]{1,40}?)\s*(?:\n|$)",
        RegexOptions.Multiline | RegexOptions.IgnoreCase);

    // Lieu de naissance ancienne CNI ("à : PARIS 1ER")
    private static readonly Regex RxCniLieuNaissanceOld = new(
        @"\bà\s*:\s*([A-ZÀÂÉÈÊËÎÏÔÙÛÜÇ][A-ZÀÂÉÈÊËÎÏÔÙÛÜÇ\s0-9\(\)]{1,30}?)\s*(?:\n|$)",
        RegexOptions.Multiline);

    // Nettoie un lieu de naissance : supprime un éventuel caractère parasite en début
    // ex. "H ORLEANS" (H = fin du mot BIRTH lu par l'OCR) → "ORLEANS"
    private static readonly Regex RxLieuBruitDebut = new(@"^[A-Z]\s+");

    // MRZ ligne 2 : commence par YYMMDD + check + sexe (M/F)
    private static readonly Regex RxMrzLigne2 = new(
        @"^(\d{2})(\d{2})(\d{2})\d[MF]",
        RegexOptions.Multiline);

    // MRZ ligne 3 (BLO) : NOM<<PRÉNOM1<PRÉNOM2<<<
    private static readonly Regex RxMrzLigne3 = new(
        @"^([A-Z]{2,30})<<([A-Z]+(?:<[A-Z]+)*)<*\s*$",
        RegexOptions.Multiline);

    public ProprietaireScanPage(bool avecNaissance = false)
    {
        _avecNaissance = avecNaissance;
        InitializeComponent();
        if (_avecNaissance)
        {
            IconDateNaissance.IsVisible = true;
            TextDateNaissance.IsVisible = true;
            IconLieuNaissance.IsVisible = true;
            TextLieuNaissance.IsVisible = true;
        }
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
        while (!ct.IsCancellationRequested && !_data.IsComplete(_avecNaissance))
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

            var texte = await OcrService.ReconnaitreTexteAsync(bytes);
            if (string.IsNullOrWhiteSpace(texte)) return;

            ExtractFields(texte);

            if (_data.IsComplete(_avecNaissance))
                await CloseAsync();
        }
        catch { }
    }

    private void ExtractFields(string texte)
    {
        // Nom — carte grise C.1, sinon CNI NOM label
        if (_data.Nom == null)
        {
            var m = RxCgNom.Match(texte);
            if (m.Success)
            {
                _data.Nom = m.Groups[1].Value.Trim().ToUpperInvariant();
                SetTrouve(IconNom, TextNom, "Nom", _data.Nom);
            }
            else
            {
                m = RxCniNom.Match(texte);
                if (m.Success)
                {
                    _data.Nom = m.Groups[1].Value.Trim().ToUpperInvariant();
                    SetTrouve(IconNom, TextNom, "Nom", _data.Nom);
                }
            }
        }

        // Prénom — C4.1, sinon ligne après C.1, sinon CNI PRÉNOM label
        if (_data.Prenom == null)
        {
            var m = RxCgPrenomApresC41.Match(texte);
            if (!m.Success) m = RxCgPrenomApresC1.Match(texte);
            if (m.Success)
            {
                _data.Prenom = m.Groups[1].Value.Trim();
                SetTrouve(IconPrenom, TextPrenom, "Prénom(s)", _data.Prenom);
            }
            else
            {
                m = RxCniPrenom.Match(texte);
                if (m.Success)
                {
                    _data.Prenom = m.Groups[1].Value.Split(',')[0].Trim();
                    SetTrouve(IconPrenom, TextPrenom, "Prénom(s)", _data.Prenom);
                }
            }
        }

        // MRZ fallback (BLO ligne 3) — si nom ou prénom encore manquant
        if (_data.Nom == null || _data.Prenom == null)
        {
            var m = RxMrzLigne3.Match(texte);
            if (m.Success)
            {
                if (_data.Nom == null)
                {
                    _data.Nom = m.Groups[1].Value.Replace('<', ' ').Trim().ToUpperInvariant();
                    SetTrouve(IconNom, TextNom, "Nom", _data.Nom);
                }
                if (_data.Prenom == null)
                {
                    _data.Prenom = m.Groups[2].Value.Split('<')[0].Trim();
                    SetTrouve(IconPrenom, TextPrenom, "Prénom(s)", _data.Prenom);
                }
            }
        }

        // Adresse — type de voie direct, sinon ancre ADRESSE (CNI dos)
        if (_data.AdresseNumVoie == null)
        {
            var m = RxCgAdresse.Match(texte);
            if (m.Success)
            {
                _data.AdresseNumVoie = m.Groups[1].Value.Trim();
                SetTrouve(IconAdresse, TextAdresse, "Adresse", _data.AdresseNumVoie);
            }
            else
            {
                m = RxCniAdresse.Match(texte);
                if (m.Success)
                {
                    _data.AdresseNumVoie = m.Groups[1].Value.Trim();
                    SetTrouve(IconAdresse, TextAdresse, "Adresse", _data.AdresseNumVoie);
                }
            }
        }

        // Code postal + commune
        if (_data.CodePostal == null)
        {
            var m = RxCgCPCommune.Match(texte);
            if (m.Success)
            {
                _data.CodePostal = m.Groups[1].Value.Trim();
                _data.Commune    = m.Groups[2].Value.Trim().ToUpperInvariant();
                SetTrouve(IconCP,      TextCP,      "Code postal", _data.CodePostal);
                SetTrouve(IconCommune, TextCommune, "Commune",     _data.Commune);
            }
        }

        // Date de naissance — label CNI, sinon MRZ ligne 2
        if (_data.DateNaissance == null)
        {
            var m = RxCniDateNaissance.Match(texte);
            if (m.Success)
            {
                _data.DateNaissance = NormaliserDate(m.Groups[1].Value);
                SetTrouve(IconDateNaissance, TextDateNaissance, "Naissance", _data.DateNaissance);
            }
            else
            {
                m = RxMrzLigne2.Match(texte);
                if (m.Success)
                {
                    _data.DateNaissance = DateDepuisMrz(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value);
                    SetTrouve(IconDateNaissance, TextDateNaissance, "Naissance", _data.DateNaissance);
                }
            }
        }

        // Lieu de naissance — label CNI nouvelle, sinon ancienne CNI ("à :")
        if (_data.LieuNaissance == null)
        {
            var m = RxCniLieuNaissance.Match(texte);
            if (!m.Success) m = RxCniLieuNaissanceOld.Match(texte);
            if (m.Success)
            {
                var lieu = RxLieuBruitDebut.Replace(m.Groups[1].Value.Trim(), "").ToUpperInvariant();
                _data.LieuNaissance = lieu;
                SetTrouve(IconLieuNaissance, TextLieuNaissance, "Lieu naissance", _data.LieuNaissance);
            }
        }
    }

    private static string NormaliserDate(string raw)
        => raw.Replace(' ', '/').Replace('.', '/');

    private static string DateDepuisMrz(string yy, string mm, string dd)
    {
        int y = int.Parse(yy);
        int fullYear = y <= DateTime.Now.Year % 100 ? 2000 + y : 1900 + y;
        return $"{dd}/{mm}/{fullYear}";
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
}
