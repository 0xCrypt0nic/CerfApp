namespace CessionApp.Models;

public class AncienProprietaireData : ProprietaireBaseData
{
    public bool CederPourDestruction { get; set; }  // false = Céder, true = Céder pour destruction
    public string? DateCession { get; set; }
    public string? HeureCession { get; set; }
    public string? MinuteCession { get; set; }

    // ── Je certifie en outre ─────────────────────────────────────────────────
    public bool CertificatSituationAdmin { get; set; }
    public bool PasTransformationNotable { get; set; }
    public bool CessionVhu { get; set; }
    public string? NumeroAgrementVhu { get; set; }
}
