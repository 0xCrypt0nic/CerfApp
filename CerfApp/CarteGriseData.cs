namespace CerfApp;

public class CarteGriseData
{
    public string? Immatriculation { get; set; }
    public string? Vin { get; set; }
    public string? Marque { get; set; }
    public string? Type { get; set; }
    public string? Denomination { get; set; }
    public string? Genre { get; set; }
    public string? DateImmat { get; set; }

    public string? NumeroFormule { get; set; }
    public string? DateCertificat { get; set; } // champ (I) — ancien format uniquement
    public bool EstAncienFormat { get; set; }

    public bool IsComplete(bool avecNumeroFormule) =>
        Immatriculation != null && Vin != null && Marque != null &&
        Type != null && Denomination != null && Genre != null && DateImmat != null &&
        (EstAncienFormat
            ? DateCertificat != null
            : !avecNumeroFormule || NumeroFormule != null);
}
