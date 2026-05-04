namespace CerfApp.Models;

public class Cerfa13750PersonneData
{
    public bool   EstPersonneMorale { get; set; }
    public string Sexe              { get; set; } = "M";

    public string? NomPrenom { get; set; }
    public string? NomUsage  { get; set; }
    public string? Siret     { get; set; }

    // Naissance (personne physique uniquement)
    public string? DateNaissance  { get; set; }   // JJ/MM/AAAA
    public string? VilleNaissance { get; set; }
    public string? DepNaissance   { get; set; }
    public string? PaysNaissance  { get; set; }

    // Adresse
    public string? Etage     { get; set; }
    public string? Immeuble  { get; set; }
    public string? NumVoie   { get; set; }
    public string? Extension { get; set; }
    public string? TypeVoie  { get; set; }
    public string? NomVoie   { get; set; }
    public string? LieuDit   { get; set; }
    public string? CodePostal { get; set; }
    public string? Commune   { get; set; }
    public string? Telephone { get; set; }
    public string? Mail      { get; set; }

    // Signature
    public string? VilleSignature { get; set; }
    public string? DateSignature  { get; set; }   // JJ/MM/AAAA

    public bool Opposition { get; set; }
}

public class Cerfa13750TitulaireData : Cerfa13750PersonneData
{
    public string? NbCotitulaires   { get; set; }
    public string? CotitulaireNom   { get; set; }
    public string? CotitulaireSiret { get; set; }
}
