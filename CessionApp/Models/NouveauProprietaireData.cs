namespace CessionApp.Models;

public class NouveauProprietaireData : ProprietaireBaseData
{
    public string? DateNaissance { get; set; }
    public string? LieuNaissance { get; set; }

    // ── Certifie ─────────────────────────────────────────────────────────────
    public bool CertifieAcquerir { get; set; }
    public bool CertifieInformeSituation { get; set; }
}
