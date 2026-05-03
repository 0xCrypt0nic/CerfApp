using CerfApp.Models;

namespace CerfApp;

public partial class NouveauProprietairePage : ContentPage
{
    private readonly CessionData _cession;
    private ProprietaireScanPage? _pendingScan;
    private static bool _popupAffichee;

    public NouveauProprietairePage(CessionData cession)
    {
        _cession = cession;
        InitializeComponent();
        ChargerDepuisModele();
    }

    private void ChargerDepuisModele()
    {
        var p = _cession.NouveauProprietaire;
        if (p.EstPersonneMorale == true)
            PersonneMoraleRadio.IsChecked = true;
        else
            PersonnePhysiqueRadio.IsChecked = true;
        if (p.Sexe == "M") SexeMRadio.IsChecked = true;
        if (p.Sexe == "F") SexeFRadio.IsChecked = true;
        SexeLayout.IsVisible      = PersonnePhysiqueRadio.IsChecked;
        NomCompletEntry.Text      = p.NomComplet;
        SiretEntry.Text           = p.Siret;
        DateNaissanceEntry.Text   = p.DateNaissance;
        LieuNaissanceEntry.Text   = p.LieuNaissance;
        AdresseNumVoieEntry.Text  = p.AdresseNumVoie;
        CodePostalEntry.Text      = p.CodePostal;
        CommuneEntry.Text         = p.Commune;
        CheckAcquerir.IsChecked         = p.CertifieAcquerir;
        CheckInformeSituation.IsChecked = p.CertifieInformeSituation;
        DateFaitEntry.Text        = p.DateFait;
        LieuFaitEntry.Text        = p.LieuFait;
    }

    private void SauvegarderDansModele()
    {
        var p = _cession.NouveauProprietaire;
        p.EstPersonneMorale = PersonneMoraleRadio.IsChecked   ? true
                            : PersonnePhysiqueRadio.IsChecked ? false
                            : null;
        p.Sexe              = SexeMRadio.IsChecked ? "M" : SexeFRadio.IsChecked ? "F" : null;
        p.NomComplet        = NomCompletEntry.Text;
        p.Siret             = SiretEntry.Text;
        p.DateNaissance     = DateNaissanceEntry.Text;
        p.LieuNaissance     = LieuNaissanceEntry.Text;
        p.AdresseNumVoie    = AdresseNumVoieEntry.Text;
        p.CodePostal        = CodePostalEntry.Text;
        p.Commune           = CommuneEntry.Text;
        p.CertifieAcquerir         = CheckAcquerir.IsChecked;
        p.CertifieInformeSituation = CheckInformeSituation.IsChecked;
        p.DateFait          = DateFaitEntry.Text;
        p.LieuFait          = LieuFaitEntry.Text;
    }

    private void OnTypeChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;
        SexeLayout.IsVisible = PersonnePhysiqueRadio.IsChecked;
    }


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_popupAffichee)
        {
            _popupAffichee = true;
            var reponse = await DisplayAlertAsync(
                "Remplissage automatique",
                "Voulez-vous renseigner automatiquement la date et le lieu ?",
                "Oui", "Non");

            if (reponse)
                await RemplirDateEtLieuAsync();
        }

        if (_pendingScan != null)
        {
            var r = _pendingScan.Result;
            if (r.Nom != null || r.Prenom != null)
                NomCompletEntry.Text = $"{r.Nom} {r.Prenom}".Trim();
            if (r.AdresseNumVoie != null)  AdresseNumVoieEntry.Text  = r.AdresseNumVoie;
            if (r.CodePostal != null)      CodePostalEntry.Text      = r.CodePostal;
            if (r.Commune != null)         CommuneEntry.Text         = r.Commune;
            if (r.DateNaissance != null)   DateNaissanceEntry.Text   = r.DateNaissance;
            if (r.LieuNaissance != null)   LieuNaissanceEntry.Text   = r.LieuNaissance;
            _pendingScan = null;
        }
    }

    private async Task RemplirDateEtLieuAsync()
    {
        DateFaitEntry.Text = DateTime.Now.ToString("dd/MM/yyyy");

        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted) return;

            var location = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));

            if (location == null) return;

            var placemarks = await Geocoding.Default.GetPlacemarksAsync(
                location.Latitude, location.Longitude);

            var commune = placemarks?.FirstOrDefault()?.Locality;
            if (!string.IsNullOrWhiteSpace(commune))
                LieuFaitEntry.Text = commune.ToUpperInvariant();
        }
        catch { }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        SauvegarderDansModele();
    }

    private void OnLabelAcquerirTapped(object? sender, TappedEventArgs e)
        => CheckAcquerir.IsChecked = !CheckAcquerir.IsChecked;

    private void OnLabelInformeSituationTapped(object? sender, TappedEventArgs e)
        => CheckInformeSituation.IsChecked = !CheckInformeSituation.IsChecked;

    private async void OnScanClicked(object? sender, EventArgs e)
    {
        _pendingScan = new ProprietaireScanPage(avecNaissance: true);
        await Navigation.PushAsync(_pendingScan);
    }

    private async void OnSuivantClicked(object? sender, EventArgs e)
    {
        SauvegarderDansModele();
        await Navigation.PushAsync(new RecapitulatifPage(_cession));
    }
}
