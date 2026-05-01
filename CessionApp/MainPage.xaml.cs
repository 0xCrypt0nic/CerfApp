using CessionApp.Models;

namespace CessionApp;

public partial class MainPage : ContentPage
{
    private readonly CessionData _cession;
    private ScanPage? _pendingScan;
    private bool _alerteAffichee;
    private static bool _kilometreDemandeeFait;

    public MainPage(CessionData cession)
    {
        _cession = cession;
        InitializeComponent();
    }

    private void OnCertificatChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;
        NumeroFormuleLayout.IsVisible  = CertOuiRadio.IsChecked;
        DateCertificatLayout.IsVisible = false;
        MotifAbsenceLayout.IsVisible   = CertNonRadio.IsChecked;
        ScanButton.IsEnabled           = CertOuiRadio.IsChecked;
    }

    private async void OnScanClicked(object sender, EventArgs e)
    {
        _pendingScan = new ScanPage(avecNumeroFormule: true);
        await Navigation.PushAsync(_pendingScan);
    }

    private async void OnSuivantClicked(object sender, EventArgs e)
    {
        SauvegarderDansModele();

        if (!_kilometreDemandeeFait)
        {
            _kilometreDemandeeFait = true;
            var km = await DisplayPromptAsync(
                "Kilométrage",
                "Kilométrage inscrit au compteur du véhicule :",
                accept: "Valider",
                cancel: "Passer",
                placeholder: "ex : 125000",
                maxLength: 10,
                keyboard: Keyboard.Numeric);

            if (!string.IsNullOrWhiteSpace(km))
                _cession.Vehicule.Kilometrage = km.Trim();
        }

        await Navigation.PushAsync(new AncienProprietairePage(_cession));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_alerteAffichee)
        {
            _alerteAffichee = true;
            await DisplayAlertAsync(
                "Information",
                "Documents éligibles au scan :\n\n" +
                "  🚗  Véhicule\n" +
                "       Carte grise\n\n" +
                "  👤  Ancien propriétaire\n" +
                "       Carte grise  •  Carte d'identité\n\n" +
                "  👤  Nouveau propriétaire\n" +
                "       Carte d'identité\n\n\n" +
                "Le scan OCR peut produire des erreurs.\n" +
                "Vérifiez chaque champ après le scan :\n" +
                "  • chiffre 5 / lettre S\n" +
                "  • chiffre 0 / lettre O\n" +
                "  • chiffre 1 / lettre I",
                "Compris");
        }

        if (_pendingScan != null)
        {
            var r = _pendingScan.Result;
            if (r.Immatriculation != null) ImmatriculationEntry.Text  = r.Immatriculation;
            if (r.Vin != null)             VinEntry.Text              = r.Vin;
            if (r.Marque != null)          MarqueEntry.Text           = r.Marque;
            if (r.Type != null)            TypeEntry.Text             = r.Type;
            if (r.Denomination != null)    DenominationEntry.Text     = r.Denomination;
            if (r.Genre != null)           GenreEntry.Text            = r.Genre;
            if (r.DateImmat != null)       DateImmatEntry.Text        = r.DateImmat;
            if (r.NumeroFormule != null)   NumeroFormuleEntry.Text    = r.NumeroFormule;
            if (r.DateCertificat != null)
            {
                DateCertificatEntry.Text       = r.DateCertificat;
                NumeroFormuleLayout.IsVisible  = false;
                DateCertificatLayout.IsVisible = true;
            }
            _pendingScan = null;
        }
    }

    private void SauvegarderDansModele()
    {
        var v = _cession.Vehicule;
        v.Immatriculation   = ImmatriculationEntry.Text;
        v.DateImmat         = DateImmatEntry.Text;
        v.Marque            = MarqueEntry.Text;
        v.Type              = TypeEntry.Text;
        v.Denomination      = DenominationEntry.Text;
        v.Vin               = VinEntry.Text;
        v.Genre             = GenreEntry.Text;
        v.CertificatPresent = CertOuiRadio.IsChecked;
        v.NumeroFormule     = NumeroFormuleEntry.Text;
        v.DateCertificat    = DateCertificatEntry.Text;
        v.MotifAbsence      = MotifAbsenceEditor.Text;
    }
}
