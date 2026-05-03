namespace CerfApp.Models;

public class ProprietaireOcrData
{
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public string? AdresseNumVoie { get; set; }
    public string? CodePostal { get; set; }
    public string? Commune { get; set; }
    public string? DateNaissance { get; set; }
    public string? LieuNaissance { get; set; }

    public bool IsComplete(bool avecNaissance = false)
        => Nom != null && Prenom != null
        && AdresseNumVoie != null && CodePostal != null && Commune != null
        && (!avecNaissance || (DateNaissance != null && LieuNaissance != null));
}
