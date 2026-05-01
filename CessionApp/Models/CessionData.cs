namespace CessionApp.Models;

public class CessionData
{
    public VehiculeData           Vehicule           { get; } = new();
    public AncienProprietaireData AncienProprietaire { get; } = new();
    public NouveauProprietaireData NouveauProprietaire { get; } = new();
}
