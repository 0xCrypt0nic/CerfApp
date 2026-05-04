using CerfApp.Models;

namespace CerfApp;

public partial class Cerfa13750TypePage : ContentPage
{
    private readonly Cerfa13750Data _data;

    public Cerfa13750TypePage(Cerfa13750Data data)
    {
        InitializeComponent();
        _data = data;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RadioCertificat.IsChecked           = _data.TypeDemande == TypeDemande13750.Certificat;
        RadioDuplicata.IsChecked            = _data.TypeDemande == TypeDemande13750.Duplicata;
        RadioCorrection.IsChecked           = _data.TypeDemande == TypeDemande13750.Correction;
        RadioChangementDomicile.IsChecked   = _data.TypeDemande == TypeDemande13750.ChangementDomicile;
        RadioChangementEtatCivil.IsChecked  = _data.TypeDemande == TypeDemande13750.ChangementEtatCivil;
        RadioChangementTechnique.IsChecked  = _data.TypeDemande == TypeDemande13750.ChangementTechnique;
    }

    private void OnTypeChanged(object sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;
        if (sender == RadioCertificat)          _data.TypeDemande = TypeDemande13750.Certificat;
        else if (sender == RadioDuplicata)      _data.TypeDemande = TypeDemande13750.Duplicata;
        else if (sender == RadioCorrection)     _data.TypeDemande = TypeDemande13750.Correction;
        else if (sender == RadioChangementDomicile)  _data.TypeDemande = TypeDemande13750.ChangementDomicile;
        else if (sender == RadioChangementEtatCivil) _data.TypeDemande = TypeDemande13750.ChangementEtatCivil;
        else if (sender == RadioChangementTechnique) _data.TypeDemande = TypeDemande13750.ChangementTechnique;
    }

    private async void OnSuivantClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Cerfa13750VehiculePage(_data));
    }
}
