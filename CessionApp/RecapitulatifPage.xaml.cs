using CessionApp.Models;
using CessionApp.Services;

namespace CessionApp;

public partial class RecapitulatifPage : ContentPage
{
    private readonly CessionData _cession;

    public RecapitulatifPage(CessionData cession)
    {
        _cession = cession;
        InitializeComponent();
        ChargerDepuisModele();
    }

    // ── Chargement ────────────────────────────────────────────────────────────

    private void ChargerDepuisModele()
    {
        ChargerVehicule();
        ChargerAncienProprietaire();
        ChargerNouveauProprietaire();
    }

    private void ChargerVehicule()
    {
        var v = _cession.Vehicule;
        V_CertOuiRadio.IsChecked       = v.CertificatPresent;
        V_CertNonRadio.IsChecked       = !v.CertificatPresent;
        V_ImmatriculationEntry.Text    = v.Immatriculation;
        V_DateImmatEntry.Text          = v.DateImmat;
        V_MarqueEntry.Text             = v.Marque;
        V_TypeEntry.Text               = v.Type;
        V_DenominationEntry.Text       = v.Denomination;
        V_VinEntry.Text                = v.Vin;
        V_GenreEntry.Text              = v.Genre;
        V_KilometrageEntry.Text        = v.Kilometrage;
        V_NumeroFormuleEntry.Text      = v.NumeroFormule;
        V_DateCertificatEntry.Text     = v.DateCertificat;
        V_MotifAbsenceEditor.Text      = v.MotifAbsence;

        if (v.CertificatPresent)
        {
            V_NumeroFormuleLayout.IsVisible  = !string.IsNullOrEmpty(v.NumeroFormule);
            V_DateCertificatLayout.IsVisible = !string.IsNullOrEmpty(v.DateCertificat);
        }
        else
        {
            V_MotifAbsenceLayout.IsVisible = true;
        }
    }

    private void ChargerAncienProprietaire()
    {
        var p = _cession.AncienProprietaire;
        if (p.EstPersonneMorale == true)
            AP_PersonneMoraleRadio.IsChecked = true;
        else
            AP_PersonnePhysiqueRadio.IsChecked = true;
        if (p.Sexe == "M") AP_SexeMRadio.IsChecked = true;
        if (p.Sexe == "F") AP_SexeFRadio.IsChecked = true;
        AP_SexeLayout.IsVisible         = AP_PersonnePhysiqueRadio.IsChecked;
        AP_NomCompletEntry.Text         = p.NomComplet;
        AP_SiretEntry.Text              = p.Siret;
        AP_AdresseEntry.Text            = p.AdresseNumVoie;
        AP_CodePostalEntry.Text         = p.CodePostal;
        AP_CommuneEntry.Text            = p.Commune;
        AP_CederDestructionRadio.IsChecked = p.CederPourDestruction;
        AP_CederRadio.IsChecked            = !p.CederPourDestruction;
        AP_DateCessionEntry.Text        = p.DateCession;
        AP_HeureEntry.Text              = p.HeureCession;
        AP_MinuteEntry.Text             = p.MinuteCession;
        AP_CheckSitAdmin.IsChecked      = p.CertificatSituationAdmin;
        AP_CheckTransformation.IsChecked = p.PasTransformationNotable;
        AP_VhuSection.IsVisible         = p.CederPourDestruction;
        AP_CheckVhu.IsChecked           = p.CessionVhu;
        AP_AgrémentEntry.Text           = p.NumeroAgrementVhu;
        AP_DateFaitEntry.Text           = p.DateFait;
        AP_LieuFaitEntry.Text           = p.LieuFait;
    }

    private void ChargerNouveauProprietaire()
    {
        var p = _cession.NouveauProprietaire;
        if (p.EstPersonneMorale == true)
            NP_PersonneMoraleRadio.IsChecked = true;
        else
            NP_PersonnePhysiqueRadio.IsChecked = true;
        if (p.Sexe == "M") NP_SexeMRadio.IsChecked = true;
        if (p.Sexe == "F") NP_SexeFRadio.IsChecked = true;
        NP_SexeLayout.IsVisible          = NP_PersonnePhysiqueRadio.IsChecked;
        NP_NomCompletEntry.Text          = p.NomComplet;
        NP_SiretEntry.Text               = p.Siret;
        NP_DateNaissanceEntry.Text       = p.DateNaissance;
        NP_LieuNaissanceEntry.Text       = p.LieuNaissance;
        NP_AdresseEntry.Text             = p.AdresseNumVoie;
        NP_CodePostalEntry.Text          = p.CodePostal;
        NP_CommuneEntry.Text             = p.Commune;
        NP_CheckAcquerir.IsChecked       = p.CertifieAcquerir;
        NP_CheckInformeSituation.IsChecked = p.CertifieInformeSituation;
        NP_DateFaitEntry.Text            = p.DateFait;
        NP_LieuFaitEntry.Text            = p.LieuFait;
    }

    // ── Sauvegarde ────────────────────────────────────────────────────────────

    private void SauvegarderDansModele()
    {
        SauvegarderVehicule();
        SauvegarderAncienProprietaire();
        SauvegarderNouveauProprietaire();
    }

    private void SauvegarderVehicule()
    {
        var v = _cession.Vehicule;
        v.CertificatPresent = V_CertOuiRadio.IsChecked;
        v.Immatriculation   = V_ImmatriculationEntry.Text;
        v.DateImmat         = V_DateImmatEntry.Text;
        v.Marque            = V_MarqueEntry.Text;
        v.Type              = V_TypeEntry.Text;
        v.Denomination      = V_DenominationEntry.Text;
        v.Vin               = V_VinEntry.Text;
        v.Genre             = V_GenreEntry.Text;
        v.Kilometrage       = V_KilometrageEntry.Text;
        v.NumeroFormule     = V_NumeroFormuleEntry.Text;
        v.DateCertificat    = V_DateCertificatEntry.Text;
        v.MotifAbsence      = V_MotifAbsenceEditor.Text;
    }

    private void SauvegarderAncienProprietaire()
    {
        var p = _cession.AncienProprietaire;
        p.EstPersonneMorale    = AP_PersonneMoraleRadio.IsChecked ? true
                               : AP_PersonnePhysiqueRadio.IsChecked ? false : null;
        p.Sexe                 = AP_SexeMRadio.IsChecked ? "M" : AP_SexeFRadio.IsChecked ? "F" : null;
        p.NomComplet           = AP_NomCompletEntry.Text;
        p.Siret                = AP_SiretEntry.Text;
        p.AdresseNumVoie       = AP_AdresseEntry.Text;
        p.CodePostal           = AP_CodePostalEntry.Text;
        p.Commune              = AP_CommuneEntry.Text;
        p.CederPourDestruction = AP_CederDestructionRadio.IsChecked;
        p.DateCession          = AP_DateCessionEntry.Text;
        p.HeureCession         = AP_HeureEntry.Text;
        p.MinuteCession        = AP_MinuteEntry.Text;
        p.CertificatSituationAdmin = AP_CheckSitAdmin.IsChecked;
        p.PasTransformationNotable = AP_CheckTransformation.IsChecked;
        p.CessionVhu           = AP_CheckVhu.IsChecked;
        p.NumeroAgrementVhu    = AP_CheckVhu.IsChecked ? AP_AgrémentEntry.Text : null;
        p.DateFait             = AP_DateFaitEntry.Text;
        p.LieuFait             = AP_LieuFaitEntry.Text;
    }

    private void SauvegarderNouveauProprietaire()
    {
        var p = _cession.NouveauProprietaire;
        p.EstPersonneMorale    = NP_PersonneMoraleRadio.IsChecked ? true
                               : NP_PersonnePhysiqueRadio.IsChecked ? false : null;
        p.Sexe                 = NP_SexeMRadio.IsChecked ? "M" : NP_SexeFRadio.IsChecked ? "F" : null;
        p.NomComplet           = NP_NomCompletEntry.Text;
        p.Siret                = NP_SiretEntry.Text;
        p.DateNaissance        = NP_DateNaissanceEntry.Text;
        p.LieuNaissance        = NP_LieuNaissanceEntry.Text;
        p.AdresseNumVoie       = NP_AdresseEntry.Text;
        p.CodePostal           = NP_CodePostalEntry.Text;
        p.Commune              = NP_CommuneEntry.Text;
        p.CertifieAcquerir         = NP_CheckAcquerir.IsChecked;
        p.CertifieInformeSituation = NP_CheckInformeSituation.IsChecked;
        p.DateFait             = NP_DateFaitEntry.Text;
        p.LieuFait             = NP_LieuFaitEntry.Text;
    }

    // ── Handlers dynamiques ───────────────────────────────────────────────────

    private void OnCertificatChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;
        V_NumeroFormuleLayout.IsVisible  = V_CertOuiRadio.IsChecked;
        V_DateCertificatLayout.IsVisible = false;
        V_MotifAbsenceLayout.IsVisible   = V_CertNonRadio.IsChecked;
    }

    private void OnTypeAncienChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;
        AP_SexeLayout.IsVisible = AP_PersonnePhysiqueRadio.IsChecked;
    }

    private void OnCederChanged(object? sender, CheckedChangedEventArgs e)
    {
        AP_VhuSection.IsVisible = AP_CederDestructionRadio.IsChecked;
        if (!AP_CederDestructionRadio.IsChecked)
        {
            AP_CheckVhu.IsChecked  = false;
            AP_AgrémentEntry.Text  = null;
        }
    }

    private void OnTypeNouveauChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (!e.Value) return;
        NP_SexeLayout.IsVisible = NP_PersonnePhysiqueRadio.IsChecked;
    }

    private void OnLabelAPSitAdminTapped(object? sender, TappedEventArgs e)
        => AP_CheckSitAdmin.IsChecked = !AP_CheckSitAdmin.IsChecked;

    private void OnLabelAPTransformationTapped(object? sender, TappedEventArgs e)
        => AP_CheckTransformation.IsChecked = !AP_CheckTransformation.IsChecked;

    private void OnLabelAPVhuTapped(object? sender, TappedEventArgs e)
        => AP_CheckVhu.IsChecked = !AP_CheckVhu.IsChecked;

    private void OnLabelNPAcquerirTapped(object? sender, TappedEventArgs e)
        => NP_CheckAcquerir.IsChecked = !NP_CheckAcquerir.IsChecked;

    private void OnLabelNPInformeSituationTapped(object? sender, TappedEventArgs e)
        => NP_CheckInformeSituation.IsChecked = !NP_CheckInformeSituation.IsChecked;


    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        SauvegarderDansModele();
    }

    private async void OnGenererClicked(object? sender, EventArgs e)
    {
        SauvegarderDansModele();

        try
        {
            var pdfPath = await CerfaGenerator.GenererAsync(_cession);
            await Navigation.PushAsync(new PdfViewerPage(pdfPath));
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erreur", $"Impossible de générer le PDF :\n{ex.Message}", "OK");
        }
    }

    private async void OnSignerEtGenererClicked(object? sender, EventArgs e)
    {
        SauvegarderDansModele();
        await Navigation.PushAsync(new SignaturePage(_cession, isVendeur: true));
    }
}
