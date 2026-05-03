namespace CerfApp.Models;

public class VehiculeData
{
    public string? Immatriculation  { get; set; }
    public string? DateImmat        { get; set; }
    public string? Marque           { get; set; }
    public string? Type             { get; set; }
    public string? Denomination     { get; set; }
    public string? Vin              { get; set; }
    public string? Genre            { get; set; }
    public bool    CertificatPresent { get; set; }
    public string? NumeroFormule    { get; set; }
    public string? DateCertificat   { get; set; }
    public string? MotifAbsence     { get; set; }
    public string? Kilometrage      { get; set; }
}
