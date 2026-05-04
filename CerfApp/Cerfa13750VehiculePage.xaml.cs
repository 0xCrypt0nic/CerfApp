using CerfApp.Models;

namespace CerfApp;

public partial class Cerfa13750VehiculePage : ContentPage
{
    private readonly Cerfa13750Data _data;
    private ScanPage? _pendingScan;

    private static readonly string[] Couleurs =
        ["Non défini", "Noire", "Marron", "Rouge", "Orange", "Jaune", "Verte", "Bleue", "Beige"];

    public Cerfa13750VehiculePage(Cerfa13750Data data)
    {
        InitializeComponent();
        _data = data;
        CouleurPicker.ItemsSource = Couleurs;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_pendingScan != null)
        {
            var r = _pendingScan.Result;
            if (r.Immatriculation != null) ImmatriculationEntry.Text = r.Immatriculation;
            if (r.DateImmat       != null) MiseCircEntry.Text        = r.DateImmat;
            if (r.NumeroFormule   != null) NumFormuleEntry.Text      = r.NumeroFormule;
            if (r.DateCertificat  != null) CertEntry.Text            = r.DateCertificat;
            if (r.Marque          != null) MarqueEntry.Text          = r.Marque;
            if (r.Type            != null) ModeleEntry.Text          = r.Type;
            if (r.Denomination    != null && string.IsNullOrEmpty(ModeleEntry.Text))
                ModeleEntry.Text = r.Denomination;
            if (r.Genre           != null) GenreEntry.Text           = r.Genre;
            _pendingScan = null;
            return;
        }

        var v = _data.Vehicule;
        ImmatriculationEntry.Text   = v.Immatriculation;
        MiseCircEntry.Text          = v.DateMiseCirculation;
        CertEntry.Text              = v.DateCertificat;
        DateEntreeEntry.Text        = v.DateEntree;
        NumFormuleEntry.Text        = v.NumeroFormule;
        MarqueEntry.Text            = v.Marque;
        ModeleEntry.Text            = v.Modele;
        TypeMineEntry.Text          = v.TypeMine;
        SerieEntry.Text             = v.Serie;
        GenreEntry.Text             = v.Genre;
        NumExploitEntry.Text        = v.NumeroExploitation;
        CouleurPicker.SelectedIndex = (int)v.Couleur;
        TeinteNonRadio.IsChecked    = v.Teinte == TeinteVehicule.NonDefini;
        TeinteCLairRadio.IsChecked  = v.Teinte == TeinteVehicule.Clair;
        TeinteFonceRadio.IsChecked  = v.Teinte == TeinteVehicule.Fonce;
    }

    private void OnCouleurChanged(object sender, EventArgs e)
    {
        _data.Vehicule.Couleur = (CouleurVehicule)CouleurPicker.SelectedIndex;
    }

    private async void OnScanClicked(object sender, EventArgs e)
    {
        _pendingScan = new ScanPage(avecNumeroFormule: true);
        await Navigation.PushAsync(_pendingScan);
    }

    private void SauvegarderDansModele()
    {
        var v = _data.Vehicule;
        v.Immatriculation      = ImmatriculationEntry.Text;
        v.DateMiseCirculation  = MiseCircEntry.Text;
        v.DateCertificat       = CertEntry.Text;
        v.DateEntree           = DateEntreeEntry.Text;
        v.NumeroFormule        = NumFormuleEntry.Text;
        v.Marque               = MarqueEntry.Text;
        v.Modele               = ModeleEntry.Text;
        v.TypeMine             = TypeMineEntry.Text;
        v.Serie                = SerieEntry.Text;
        v.Genre                = GenreEntry.Text;
        v.NumeroExploitation   = NumExploitEntry.Text;
        v.Couleur              = (CouleurVehicule)Math.Max(0, CouleurPicker.SelectedIndex);
        v.Teinte = TeinteCLairRadio.IsChecked ? TeinteVehicule.Clair
                 : TeinteFonceRadio.IsChecked ? TeinteVehicule.Fonce
                 : TeinteVehicule.NonDefini;
    }

    private async void OnSuivantClicked(object sender, EventArgs e)
    {
        SauvegarderDansModele();

        var choix = await DisplayActionSheet(
            "Situation locative du véhicule ?",
            "Annuler", null,
            "Non", "Location longue durée", "Location courte durée", "Crédit-bail");

        if (choix == null || choix == "Annuler") return;

        _data.SituationLocative = choix switch
        {
            "Location longue durée" => SituationLocative.LongueDuree,
            "Location courte durée" => SituationLocative.CourteDuree,
            "Crédit-bail"           => SituationLocative.CreditBail,
            _                       => SituationLocative.Non
        };

        await Navigation.PushAsync(new Cerfa13750TitulairePage(_data));
    }
}
