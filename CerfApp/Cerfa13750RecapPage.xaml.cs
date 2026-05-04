using CerfApp.Models;

namespace CerfApp;

public partial class Cerfa13750RecapPage : ContentPage
{
    private readonly Cerfa13750Data _data;

    public Cerfa13750RecapPage(Cerfa13750Data data)
    {
        InitializeComponent();
        _data = data;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ChargerRecap();
    }

    private void ChargerRecap()
    {
        var v = _data.Vehicule;
        R_Immat.Text    = v.Immatriculation    ?? "—";
        R_Marque.Text   = v.Marque             ?? "—";
        R_Modele.Text   = v.Modele             ?? "—";
        R_MiseCirc.Text = v.DateMiseCirculation ?? "—";
        R_TypeMine.Text = v.TypeMine           ?? "—";

        var couleur = v.Couleur != CouleurVehicule.NonDefini ? v.Couleur.ToString() : "—";
        var teinte  = v.Teinte  != TeinteVehicule.NonDefini  ? $" ({v.Teinte})" : "";
        R_Couleur.Text = couleur + teinte;

        var t = _data.Titulaire;
        R_TitNom.Text      = t.NomPrenom         ?? "—";
        R_TitAdresse.Text  = FormatAdresse(t);
        R_TitNaissance.Text = !t.EstPersonneMorale ? (t.DateNaissance ?? "—") : "—";
        R_TitSign.Text     = FormatSign(t.VilleSignature, t.DateSignature);

        var avecLoueur    = _data.SituationLocative != SituationLocative.Non;
        var avecLocataire = _data.SituationLocative == SituationLocative.LongueDuree ||
                            _data.SituationLocative == SituationLocative.CreditBail;

        LoueurSection.IsVisible    = avecLoueur;
        LocataireSection.IsVisible = avecLocataire;

        if (avecLoueur)
        {
            var l = _data.Loueur;
            R_LouNom.Text     = l.NomPrenom ?? "—";
            R_LouAdresse.Text = FormatAdresse(l);
            R_LouSign.Text    = FormatSign(l.VilleSignature, l.DateSignature);
        }

        if (avecLocataire)
        {
            var l = _data.Locataire;
            R_LocNom.Text     = l.NomPrenom ?? "—";
            R_LocAdresse.Text = FormatAdresse(l);
            R_LocSign.Text    = FormatSign(l.VilleSignature, l.DateSignature);
        }
    }

    private static string FormatAdresse(Cerfa13750PersonneData p)
    {
        var parts = new[] { p.NumVoie, p.Extension, p.TypeVoie, p.NomVoie }
            .Where(s => !string.IsNullOrWhiteSpace(s));
        var voie = string.Join(" ", parts);
        var cp   = p.CodePostal ?? "";
        var com  = p.Commune    ?? "";
        var ligne2 = $"{cp} {com}".Trim();
        return string.IsNullOrWhiteSpace(voie) && string.IsNullOrWhiteSpace(ligne2)
            ? "—"
            : string.IsNullOrWhiteSpace(voie) ? ligne2
            : string.IsNullOrWhiteSpace(ligne2) ? voie
            : $"{voie}\n{ligne2}";
    }

    private static string FormatSign(string? ville, string? date)
    {
        if (string.IsNullOrWhiteSpace(ville) && string.IsNullOrWhiteSpace(date)) return "—";
        return $"{ville ?? ""} {date ?? ""}".Trim();
    }

    // ── Générer (sans signature) ─────────────────────────────────────────────

    private async void OnGenererClicked(object sender, EventArgs e)
    {
        await DemanderOppositionEtGenerer();
    }

    // ── Signer puis Générer ──────────────────────────────────────────────────

    private async void OnSignerClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Cerfa13750SignaturePage(_data));
    }

    // ── Commun ───────────────────────────────────────────────────────────────

    internal async Task DemanderOppositionEtGenerer()
    {
        bool oppTit = await DisplayAlert(
            "Opposition — Titulaire",
            "Le titulaire s'oppose-t-il à la réutilisation de ses données personnelles ?",
            "Oui, je m'oppose", "Non");
        _data.Titulaire.Opposition = oppTit;

        if (_data.SituationLocative != SituationLocative.Non)
        {
            bool oppLou = await DisplayAlert(
                "Opposition — Loueur",
                "Le loueur s'oppose-t-il à la réutilisation de ses données personnelles ?",
                "Oui, je m'oppose", "Non");
            _data.Loueur.Opposition = oppLou;
        }

        if (_data.SituationLocative == SituationLocative.LongueDuree ||
            _data.SituationLocative == SituationLocative.CreditBail)
        {
            bool oppLoc = await DisplayAlert(
                "Opposition — Locataire",
                "Le locataire s'oppose-t-il à la réutilisation de ses données personnelles ?",
                "Oui, je m'oppose", "Non");
            _data.Locataire.Opposition = oppLoc;
        }

        try
        {
            var pdfPath = await Services.Cerfa13750Generator.GenererAsync(_data);
            await Navigation.PushAsync(new PdfViewerPage(pdfPath));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de générer le PDF :\n{ex.Message}", "OK");
        }
    }
}
