using CessionApp.Models;

namespace CessionApp;

public partial class AncienProprietairePage : ContentPage
{
    private readonly CessionData _cession;
    private ProprietaireScanPage? _pendingScan;
    private static bool _popupAffichee;

    public AncienProprietairePage(CessionData cession)
    {
        _cession = cession;
        InitializeComponent();
        ChargerDepuisModele();
    }

    private void ChargerDepuisModele()
    {
        var p = _cession.AncienProprietaire;
        if (p.EstPersonneMorale == true)
            PersonneMoraleRadio.IsChecked = true;
        else
            PersonnePhysiqueRadio.IsChecked = true; // défaut
        if (p.Sexe == "M") SexeMRadio.IsChecked = true;
        if (p.Sexe == "F") SexeFRadio.IsChecked = true;
        SexeLayout.IsVisible = PersonnePhysiqueRadio.IsChecked;
        CederDestructionRadio.IsChecked = p.CederPourDestruction;
        CederRadio.IsChecked            = !p.CederPourDestruction;
        NomCompletEntry.Text     = p.NomComplet;
        SiretEntry.Text          = p.Siret;
        AdresseNumVoieEntry.Text = p.AdresseNumVoie;
CodePostalEntry.Text     = p.CodePostal;
        CommuneEntry.Text        = p.Commune;
        DateCessionEntry.Text    = p.DateCession;
        HeureEntry.Text          = p.HeureCession;
        MinuteEntry.Text         = p.MinuteCession;
        DateFaitEntry.Text       = p.DateFait;
        LieuCessionEntry.Text    = p.LieuFait;
        CheckSitAdmin.IsChecked       = p.CertificatSituationAdmin;
        CheckTransformation.IsChecked = p.PasTransformationNotable;
        VhuSection.IsVisible          = p.CederPourDestruction;
        CheckVhu.IsChecked            = p.CessionVhu;
        AgrémentEntry.Text            = p.NumeroAgrementVhu;
    }


    private void OnTypeChanged(object sender, CheckedChangedEventArgs e)
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
                "Voulez-vous renseigner automatiquement la date, l'heure et le lieu de la cession ?",
                "Oui", "Non");

            if (reponse)
                await RemplirDateHeureEtLieuAsync();
        }

        if (_pendingScan != null)
        {
            var r = _pendingScan.Result;
            if (r.Nom != null || r.Prenom != null)
                NomCompletEntry.Text = $"{r.Nom} {r.Prenom}".Trim();
            if (r.AdresseNumVoie != null) AdresseNumVoieEntry.Text = r.AdresseNumVoie;
            if (r.CodePostal != null)     CodePostalEntry.Text     = r.CodePostal;
            if (r.Commune != null)        CommuneEntry.Text        = r.Commune;
            _pendingScan = null;
        }
    }

    private async Task RemplirDateHeureEtLieuAsync()
    {
        var now = DateTime.Now;
        DateCessionEntry.Text = now.ToString("dd/MM/yyyy");
        DateFaitEntry.Text    = now.ToString("dd/MM/yyyy");
        HeureEntry.Text       = now.ToString("HH");
        MinuteEntry.Text      = now.ToString("mm");

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
                LieuCessionEntry.Text  = commune.ToUpperInvariant();
        }
        catch { }
    }

    private async void OnScanClicked(object sender, EventArgs e)
    {
        _pendingScan = new ProprietaireScanPage();
        await Navigation.PushAsync(_pendingScan);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        SauvegarderDansModele();
    }

    private void OnCederChanged(object? sender, CheckedChangedEventArgs e)
    {
        VhuSection.IsVisible = CederDestructionRadio.IsChecked;
        if (!CederDestructionRadio.IsChecked)
        {
            CheckVhu.IsChecked = false;
            AgrémentEntry.Text = null;
        }
    }

    private void OnVhuChecked(object? sender, CheckedChangedEventArgs e) { }

    private void OnLabelSitAdminTapped(object? sender, TappedEventArgs e)
        => CheckSitAdmin.IsChecked = !CheckSitAdmin.IsChecked;

    private void OnLabelTransformationTapped(object? sender, TappedEventArgs e)
        => CheckTransformation.IsChecked = !CheckTransformation.IsChecked;

    private void OnLabelVhuTapped(object? sender, TappedEventArgs e)
        => CheckVhu.IsChecked = !CheckVhu.IsChecked;

    private async void OnSuivantClicked(object sender, EventArgs e)
    {
        SauvegarderDansModele();
        await Navigation.PushAsync(new NouveauProprietairePage(_cession));
    }

    private void SauvegarderDansModele()
    {
        var p = _cession.AncienProprietaire;
        p.EstPersonneMorale = PersonneMoraleRadio.IsChecked   ? true
                            : PersonnePhysiqueRadio.IsChecked ? false
                            : null;
        p.Sexe           = SexeMRadio.IsChecked ? "M" : SexeFRadio.IsChecked ? "F" : null;
        p.CederPourDestruction = CederDestructionRadio.IsChecked;
        p.NomComplet     = NomCompletEntry.Text;
        p.Siret          = SiretEntry.Text;
        p.AdresseNumVoie = AdresseNumVoieEntry.Text;
p.CodePostal     = CodePostalEntry.Text;
        p.Commune        = CommuneEntry.Text;
        p.DateCession    = DateCessionEntry.Text;
        p.HeureCession   = HeureEntry.Text;
        p.MinuteCession  = MinuteEntry.Text;
        p.DateFait       = DateFaitEntry.Text;
        p.LieuFait       = LieuCessionEntry.Text;
        p.CertificatSituationAdmin = CheckSitAdmin.IsChecked;
        p.PasTransformationNotable = CheckTransformation.IsChecked;
        p.CessionVhu               = CheckVhu.IsChecked;
        p.NumeroAgrementVhu        = CheckVhu.IsChecked ? AgrémentEntry.Text : null;
    }
}
