namespace CerfApp.Models;

public class Cerfa13750Data
{
    public TypeDemande13750      TypeDemande       { get; set; } = TypeDemande13750.Certificat;
    public SituationLocative     SituationLocative { get; set; } = SituationLocative.Non;
    public Cerfa13750VehiculeData  Vehicule          { get; } = new();
    public Cerfa13750TitulaireData Titulaire         { get; } = new();
    public Cerfa13750PersonneData  Loueur            { get; } = new();
    public Cerfa13750PersonneData  Locataire         { get; } = new();

    public byte[]? SignatureTitulaire { get; set; }
    public byte[]? SignatureLoueur    { get; set; }
    public byte[]? SignatureLocataire { get; set; }
}
