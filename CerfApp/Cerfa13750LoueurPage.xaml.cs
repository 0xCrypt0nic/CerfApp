using CerfApp.Models;

namespace CerfApp;

public partial class Cerfa13750LoueurPage : ContentPage
{
    private readonly Cerfa13750Data _data;
    private ProprietaireScanPage?   _pendingScan;

    public Cerfa13750LoueurPage(Cerfa13750Data data)
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
            _pendingScan = null;
            return;
        }

        var l = _data.Loueur;
        PersonnePhysiqueRadio.IsChecked = !l.EstPersonneMorale;
        PersonneMoraleRadio.IsChecked   = l.EstPersonneMorale;
        SexeMRadio.IsChecked            = l.Sexe == "M";
        SexeFRadio.IsChecked            = l.Sexe == "F";
        SexeLayout.IsVisible            = !l.EstPersonneMorale;
        SiretEntry.Text      = l.Siret;
        NomPrenomEntry.Text  = l.NomPrenom;
        NomUsageEntry.Text   = l.NomUsage;
        EtageEntry.Text      = l.Etage;
        ImmeubleEntry.Text   = l.Immeuble;
        NumVoieEntry.Text    = l.NumVoie;
        ExtensionEntry.Text  = l.Extension;
        TypeVoieEntry.Text   = l.TypeVoie;
        NomVoieEntry.Text    = l.NomVoie;
        LieuDitEntry.Text    = l.LieuDit;
        CodePostalEntry.Text = l.CodePostal;
        CommuneEntry.Text    = l.Commune;
        TelephoneEntry.Text  = l.Telephone;
        MailEntry.Text       = l.Mail;
        VilleSignEntry.Text  = l.VilleSignature;
        DateSignEntry.Text   = l.DateSignature;

        SuivantButton.Text = _data.SituationLocative == SituationLocative.CourteDuree
            ? "Récapitulatif" : "Suivant";
    }

    private void OnTypePersonneChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;
        SexeLayout.IsVisible = sender == PersonnePhysiqueRadio;
    }

    private async void OnScanClicked(object sender, EventArgs e)
    {
        _pendingScan = new ProprietaireScanPage(avecNaissance: false);
        await Navigation.PushAsync(_pendingScan);
    }

    private void SauvegarderDansModele()
    {
        var l = _data.Loueur;
        l.EstPersonneMorale = PersonneMoraleRadio.IsChecked;
        l.Sexe              = SexeFRadio.IsChecked ? "F" : "M";
        l.Siret             = SiretEntry.Text;
        l.NomPrenom         = NomPrenomEntry.Text;
        l.NomUsage          = NomUsageEntry.Text;
        l.Etage             = EtageEntry.Text;
        l.Immeuble          = ImmeubleEntry.Text;
        l.NumVoie           = NumVoieEntry.Text;
        l.Extension         = ExtensionEntry.Text;
        l.TypeVoie          = TypeVoieEntry.Text;
        l.NomVoie           = NomVoieEntry.Text;
        l.LieuDit           = LieuDitEntry.Text;
        l.CodePostal        = CodePostalEntry.Text;
        l.Commune           = CommuneEntry.Text;
        l.Telephone         = TelephoneEntry.Text;
        l.Mail              = MailEntry.Text;
        l.VilleSignature    = VilleSignEntry.Text;
        l.DateSignature     = DateSignEntry.Text;
    }

    private async void OnSuivantClicked(object sender, EventArgs e)
    {
        SauvegarderDansModele();

        if (_data.SituationLocative == SituationLocative.CourteDuree)
            await Navigation.PushAsync(new Cerfa13750RecapPage(_data));
        else
            await Navigation.PushAsync(new Cerfa13750LocatairePage(_data));
    }
}
