namespace CessionApp.Models;

/// <summary>Champs communs aux deux propriétaires (ancien et nouveau).</summary>
public abstract class ProprietaireBaseData
{
    public bool? EstPersonneMorale { get; set; }  // null = non sélectionné
    public string? Sexe { get; set; }             // "M", "F", ou null
    public string? NomComplet { get; set; }
    public string? Siret { get; set; }
    public string? AdresseNumVoie { get; set; }
    public string? CodePostal { get; set; }
    public string? Commune { get; set; }
    public string? LieuFait { get; set; }
    public string? DateFait { get; set; }
}
