namespace CerfApp.Models;

public enum TypeDemande13750
{
    Certificat,
    Duplicata,
    Correction,
    ChangementDomicile,
    ChangementEtatCivil,
    ChangementTechnique
}

public enum SituationLocative
{
    Non,
    LongueDuree,
    CourteDuree,
    CreditBail
}

public enum CouleurVehicule
{
    NonDefini, Noire, Marron, Rouge, Orange, Jaune, Vert, Bleu, Beige
}

public enum TeinteVehicule
{
    NonDefini, Clair, Fonce
}

public class Cerfa13750VehiculeData
{
    public string? Immatriculation      { get; set; }
    public string? DateEntree           { get; set; }   // JJ/MM/AAAA
    public string? DateCertificat       { get; set; }   // JJ/MM/AAAA
    public string? DateMiseCirculation  { get; set; }   // JJ/MM/AAAA
    public string? NumeroFormule        { get; set; }
    public string? Marque               { get; set; }
    public string? Modele               { get; set; }
    public string? TypeMine             { get; set; }
    public string? Serie                { get; set; }
    public string? Genre                { get; set; }
    public string? NumeroExploitation   { get; set; }
    public CouleurVehicule Couleur      { get; set; } = CouleurVehicule.NonDefini;
    public TeinteVehicule  Teinte       { get; set; } = TeinteVehicule.NonDefini;
}
