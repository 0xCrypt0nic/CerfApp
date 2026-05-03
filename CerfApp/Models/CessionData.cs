namespace CerfApp.Models;

public class CessionData
{
    public VehiculeData            Vehicule            { get; } = new();
    public AncienProprietaireData  AncienProprietaire  { get; } = new();
    public NouveauProprietaireData NouveauProprietaire { get; } = new();

    public byte[]? SignatureVendeur  { get; set; }
    public byte[]? SignatureAcheteur { get; set; }
}
