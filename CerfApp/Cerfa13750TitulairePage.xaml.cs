using CerfApp.Models;

namespace CerfApp;

public partial class Cerfa13750TitulairePage : ContentPage
{
    private readonly Cerfa13750Data _data;
    private ProprietaireScanPage?   _pendingScan;

    public Cerfa13750TitulairePage(Cerfa13750Data data)
    {
        InitializeComponent();
        _data = data;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_pendingScan != null)
        {
            var r = _pendingScan.Result;
            if (r.Nom != null || r.Prenom != null) NomPrenomEntry.Text = $"{r.Nom} {r.Prenom}".Trim();
            if (r.AdresseNumVoie != null) NumVoieEntry.Text    = r.AdresseNumVoie;
            if (r.CodePostal     != null) CodePostalEntry.Text = r.CodePostal;
            if (r.Commune        != null) CommuneEntry.Text    = r.Commune;
            if (r.DateNaissance  != null) NaissanceEntry.Text  = r.DateNaissance;
            if (r.LieuNaissance  != null) VilleNaissEntry.Text = r.LieuNaissance;
            _pendingScan = null;
            return;
        }

        var t = _data.Titulaire;
        PersonnePhysiqueRadio.IsChecked = !t.EstPersonneMorale;
        PersonneMoraleRadio.IsChecked   = t.EstPersonneMorale;
        SexeMRadio.IsChecked            = t.Sexe == "M";
        SexeFRadio.IsChecked            = t.Sexe == "F";
        SexeLayout.IsVisible            = !t.EstPersonneMorale;
        NaissanceLayout.IsVisible       = !t.EstPersonneMorale;
        SiretEntry.Text        = t.Siret;
        NomPrenomEntry.Text    = t.NomPrenom;
        NomUsageEntry.Text     = t.NomUsage;
        NaissanceEntry.Text    = t.DateNaissance;
        VilleNaissEntry.Text   = t.VilleNaissance;
        DepNaissEntry.Text     = t.DepNaissance;
        PaysNaissEntry.Text    = t.PaysNaissance;
        EtageEntry.Text        = t.Etage;
        ImmeubleEntry.Text     = t.Immeuble;
        NumVoieEntry.Text      = t.NumVoie;
        ExtensionEntry.Text    = t.Extension;
        TypeVoieEntry.Text     = t.TypeVoie;
        NomVoieEntry.Text      = t.NomVoie;
        LieuDitEntry.Text      = t.LieuDit;
        CodePostalEntry.Text   = t.CodePostal;
        CommuneEntry.Text      = t.Commune;
        TelephoneEntry.Text    = t.Telephone;
        MailEntry.Text         = t.Mail;
        NbCotitEntry.Text      = t.NbCotitulaires;
        CotitNomEntry.Text     = t.CotitulaireNom;
        CotitSiretEntry.Text   = t.CotitulaireSiret;
        VilleSignEntry.Text    = t.VilleSignature;
        DateSignEntry.Text     = t.DateSignature;

        SuivantButton.Text = _data.SituationLocative == SituationLocative.Non
            ? "Récap →" : "Suivant →";
    }

    private void OnTypePersonneChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;
        var estMorale = sender == PersonneMoraleRadio;
        SexeLayout.IsVisible      = !estMorale;
        NaissanceLayout.IsVisible = !estMorale;
    }

    private async void OnScanClicked(object sender, EventArgs e)
    {
        _pendingScan = new ProprietaireScanPage(avecNaissance: !PersonneMoraleRadio.IsChecked);
        await Navigation.PushAsync(_pendingScan);
    }

    private void SauvegarderDansModele()
    {
        var t = _data.Titulaire;
        t.EstPersonneMorale = PersonneMoraleRadio.IsChecked;
        t.Sexe              = SexeFRadio.IsChecked ? "F" : "M";
        t.Siret             = SiretEntry.Text;
        t.NomPrenom         = NomPrenomEntry.Text;
        t.NomUsage          = NomUsageEntry.Text;
        t.DateNaissance     = NaissanceEntry.Text;
        t.VilleNaissance    = VilleNaissEntry.Text;
        t.DepNaissance      = DepNaissEntry.Text;
        t.PaysNaissance     = PaysNaissEntry.Text;
        t.Etage             = EtageEntry.Text;
        t.Immeuble          = ImmeubleEntry.Text;
        t.NumVoie           = NumVoieEntry.Text;
        t.Extension         = ExtensionEntry.Text;
        t.TypeVoie          = TypeVoieEntry.Text;
        t.NomVoie           = NomVoieEntry.Text;
        t.LieuDit           = LieuDitEntry.Text;
        t.CodePostal        = CodePostalEntry.Text;
        t.Commune           = CommuneEntry.Text;
        t.Telephone         = TelephoneEntry.Text;
        t.Mail              = MailEntry.Text;
        t.NbCotitulaires    = NbCotitEntry.Text;
        t.CotitulaireNom    = CotitNomEntry.Text;
        t.CotitulaireSiret  = CotitSiretEntry.Text;
        t.VilleSignature    = VilleSignEntry.Text;
        t.DateSignature     = DateSignEntry.Text;
    }

    private async void OnSuivantClicked(object sender, EventArgs e)
    {
        SauvegarderDansModele();

        if (_data.SituationLocative == SituationLocative.Non)
            await Navigation.PushAsync(new Cerfa13750RecapPage(_data));
        else
            await Navigation.PushAsync(new Cerfa13750LoueurPage(_data));
    }
}
