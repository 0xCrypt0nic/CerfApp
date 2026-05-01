using CessionApp.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace CessionApp.Services;

/// <summary>
/// Génère un CERFA 15776*02 pré-rempli à partir des données de cession.
///
/// Template : vide.pdf (AcroForm 2 pages — deux exemplaires identiques).
/// Approche  : superposition de texte sur les rects exacts des widgets AcroForm
///             (extraits via pymupdf).
///   • Champs libres  → XStringFormats.CenterLeft dans le rect du widget.
///   • Champs à cases → un caractère centré par case (Cases()), positions
///                      extraites des traits graphiques du PDF.
///
/// Coordonnées : origine haut-gauche, y croissant vers le bas (points PDF).
/// </summary>
public static class CerfaGenerator
{
    private const double Ft  = 9.0;   // texte courant (noms, communes…)
    private const double Fn  = 8.5;   // chiffres / codes
    private const double Fck = 9.0;   // X dans les cases à cocher / radio

    // ── Cases individuelles — Ligne 1 (y=110.0, h=11.5) ────────────────────
    // Positions extraites des traits graphiques via pymupdf.

    // Immatriculation : 9 cases, x=35.5–163.6
    private static readonly double[] ImmatX0 = [35.5, 49.9, 64.1, 78.3, 92.5, 106.6, 120.8, 135.0, 149.2];
    private static readonly double[] ImmatX1 = [49.9, 64.1, 78.3, 92.5, 106.6, 120.8, 135.0, 149.2, 163.6];

    // VIN : 18 cases, x=173.5–429.1
    private static readonly double[] VinX0 = [173.5, 187.9, 202.1, 216.3, 230.5, 244.6, 258.8, 273.0,
                                               287.2, 301.3, 315.5, 329.7, 343.8, 358.0, 372.2, 386.4, 400.5, 414.7];
    private static readonly double[] VinX1 = [187.9, 202.1, 216.3, 230.5, 244.6, 258.8, 273.0, 287.2,
                                               301.3, 315.5, 329.7, 343.8, 358.0, 372.2, 386.4, 400.5, 414.7, 429.1];

    // Date 1re immat — Jour (2 cases), Mois (2 cases), Année (4 cases)
    private static readonly double[] DateJX0  = [440.5, 452.1];
    private static readonly double[] DateJX1  = [452.1, 463.7];
    private static readonly double[] DateMX0  = [466.2, 477.8];
    private static readonly double[] DateMX1  = [477.8, 489.4];
    private static readonly double[] DateAX0  = [491.9, 503.5, 514.8, 526.1];
    private static readonly double[] DateAX1  = [503.5, 514.8, 526.1, 537.7];

    // Numéro de formule : 9 cases, x=170.7–298.5, y=188.0, h=11.4
    private static readonly double[] FormuleX0 = [170.7, 184.8, 199.0, 213.2, 227.4, 241.5, 255.7, 269.9, 284.0];
    private static readonly double[] FormuleX1 = [184.8, 199.0, 213.2, 227.4, 241.5, 255.7, 269.9, 284.0, 298.5];

    // ── Cases individuelles — Ancien Propriétaire ────────────────────────────

    // SIRET AP : 14 cases, x=394.5–553.8, y=289.1, h=11.5
    private static readonly double[] SiretAPX0 = [394.5, 406.1, 417.4, 428.8, 440.1, 451.5, 462.8, 474.1, 485.5, 496.8, 508.2, 519.5, 530.8, 542.2];
    private static readonly double[] SiretAPX1 = [406.1, 417.4, 428.8, 440.1, 451.5, 462.8, 474.1, 485.5, 496.8, 508.2, 519.5, 530.8, 542.2, 553.8];

    // Code postal AP : 5 cases, x=107.5–178.9, y=340.0, h=11.4
    private static readonly double[] CpAPX0 = [107.5, 121.9, 136.1, 150.3, 164.5];
    private static readonly double[] CpAPX1 = [121.9, 136.1, 150.3, 164.5, 178.9];

    // Date vente — Jour (2), Mois (2), Année (4) — y=382.5, h=11.5
    private static readonly double[] VenteJX0 = [46.8,  58.4];
    private static readonly double[] VenteJX1 = [58.4,  70.0];
    private static readonly double[] VenteMX0 = [72.5,  84.0];
    private static readonly double[] VenteMX1 = [84.0,  95.6];
    private static readonly double[] VenteAX0 = [98.1, 109.7, 121.1, 132.4];
    private static readonly double[] VenteAX1 = [109.7, 121.1, 132.4, 144.0];

    // Horaire vente — H (2 cases) et M (2 cases) — y=382.5, h=11.5
    private static readonly double[] HeureX0 = [152.9, 164.5];
    private static readonly double[] HeureX1 = [164.5, 176.1];
    private static readonly double[] MinuteX0 = [186.9, 198.4];
    private static readonly double[] MinuteX1 = [198.4, 210.0];

    // ── Cases individuelles — Nouveau Propriétaire ───────────────────────────
    // SIRET NP  : mêmes x que SIRET AP → réutilise SiretAPX0/X1, y=620.2, h=11.4
    // Code postal NP : mêmes x que CP AP → réutilise CpAPX0/X1, y=683.9, h=11.4

    // Date de naissance — Jour (2), Mois (2), Année (4) — y=642.3, h=11.5
    private static readonly double[] NaissJX0 = [70.7,  82.3];
    private static readonly double[] NaissJX1 = [82.3,  93.9];
    private static readonly double[] NaissMX0 = [96.4, 108.0];
    private static readonly double[] NaissMX1 = [108.0, 119.6];
    private static readonly double[] NaissAX0 = [122.1, 133.7, 145.0, 156.3];
    private static readonly double[] NaissAX1 = [133.7, 145.0, 156.3, 167.9];

    // ── Point d'entrée ──────────────────────────────────────────────────────

    public static async Task<string> GenererAsync(CessionData cession)
    {
        using var templateStream = await FileSystem.OpenAppPackageFileAsync("vide.pdf");
        using var ms = new MemoryStream();
        await templateStream.CopyToAsync(ms);
        ms.Position = 0;

        using var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Modify);

        // Pages 0 et 1 = deux exemplaires du même formulaire (vendeur / acheteur)
        for (int i = 0; i < Math.Min(2, doc.PageCount); i++)
        {
            using var gfx = XGraphics.FromPdfPage(doc.Pages[i]);
            RemplirPage(gfx, cession);
        }

        var outputPath = Path.Combine(FileSystem.CacheDirectory, "cerfa_genere.pdf");
        doc.Save(outputPath);
        return outputPath;
    }

    // ── Page complète ────────────────────────────────────────────────────────

    private static void RemplirPage(XGraphics gfx, CessionData d)
    {
        RemplirVehicule(gfx, d.Vehicule);
        RemplirAncienProprietaire(gfx, d.AncienProprietaire);
        RemplirNouveauProprietaire(gfx, d.NouveauProprietaire);
    }

    // ── VÉHICULE ─────────────────────────────────────────────────────────────

    private static void RemplirVehicule(XGraphics gfx, VehiculeData v)
    {
        // Immatriculation : 1 caractère par case (9 cases)
        Cases(gfx, Fn, v.Immatriculation, ImmatX0, ImmatX1, 110.0, 11.5);

        // VIN : 1 caractère par case (18 cases)
        Cases(gfx, Fn, v.Vin, VinX0, VinX1, 110.0, 11.5);

        // Date 1ère immat : Jour / Mois / Année, 1 chiffre par case
        var (dj, dm, da) = SplitDate(v.DateImmat);
        Cases(gfx, Fn, dj, DateJX0, DateJX1, 110.0, 11.5);
        Cases(gfx, Fn, dm, DateMX0, DateMX1, 110.0, 11.5);
        Cases(gfx, Fn, da, DateAX0, DateAX1, 110.0, 11.5);

        // D.1/D.2/J.1/D.3 : texte centré dans chaque zone
        // txt_MarqueVéhicule           rect=(36.3, 133.5, 163.6, 145.0)
        TxtC(gfx, Ft, v.Marque,           36.3, 133.5, 127.3, 11.5);
        // txt_TypeVarianteVersionVéhicule rect=(175.5, 133.5, 307.6, 145.0)
        TxtC(gfx, Ft, v.Type,            175.5, 133.5, 132.1, 11.5);
        // txt_GenreNational            rect=(322.4, 133.5, 431.9, 145.0)
        TxtC(gfx, Ft, v.Genre,           322.4, 133.5, 109.5, 11.5);
        // txt_DénominationCommerciale  rect=(442.6, 133.5, 557.4, 145.0)
        TxtC(gfx, Ft, v.Denomination,    442.6, 133.5, 114.8, 11.5);

        // Kilométrage : num_KilométrageCompteur rect=(206.9, 158.8, 271.7, 170.2)
        TxtC(gfx, Fn, v.Kilometrage, 206.9, 158.8, 64.8, 11.4);

        // ── Certificat d'immatriculation ─────────────────────────────────────
        if (v.CertificatPresent)
        {
            // Radio OUI  rect=(35.8, 192.7, 42.8, 199.7)
            Chk(gfx, Fck, 35.8, 192.7, 7.0, 7.0);
            // Numéro de formule : 9 cases, y=188.0, h=11.4
            Cases(gfx, Fn, v.NumeroFormule, FormuleX0, FormuleX1, 188.0, 11.4);

            var (cj, cm, ca) = SplitDate(v.DateCertificat);
            Txt(gfx, Fn, cj, 223.1, 209.4, 23.2, 11.4);   // num_DateCertificatJour
            Txt(gfx, Fn, cm, 249.3, 209.4, 23.2, 11.4);   // num_DateCertificatMois
            Txt(gfx, Fn, ca, 274.9, 209.4, 43.4, 11.4);   // num_DateCertificatAnnée
        }
        else
        {
            // Radio NON  rect=(338.0, 190.1, 345.0, 197.1)
            Chk(gfx, Fck, 338.0, 190.1, 7.0, 7.0);
            // txt_MotifAbscenceCertificat  rect=(337.3, 198.1, 557.4, 222.0)
            Txt(gfx, Fn, v.MotifAbsence, 337.3, 198.1, 220.1, 23.9);
        }
    }

    // ── ANCIEN PROPRIÉTAIRE ───────────────────────────────────────────────────

    private static void RemplirAncienProprietaire(XGraphics gfx, AncienProprietaireData p)
    {
        // ── Type de personne ─────────────────────────────────────────────────
        // Physique rect=(36.3, 263.2, 43.3, 270.2) | Morale rect=(36.2, 272.7, 43.2, 279.7)
        if (p.EstPersonneMorale == true)
            Chk(gfx, Fck, 36.2, 272.7, 7.0, 7.0);
        else
            Chk(gfx, Fck, 36.3, 263.2, 7.0, 7.0);

        // Sexe M rect=(261.5, 262.7, 268.5, 269.7) | F rect=(285.6, 262.7, 292.6, 269.7)
        if (p.Sexe == "M") Chk(gfx, Fck, 261.5, 262.7, 7.0, 7.0);
        if (p.Sexe == "F") Chk(gfx, Fck, 285.6, 262.7, 7.0, 7.0);

        // ── Identité / SIRET ─────────────────────────────────────────────────
        // txt_IdentitéVendeur  rect=(91.0, 289.1, 375.4, 300.6)  — centré
        TxtC(gfx, Ft, p.NomComplet, 91.0, 289.1, 284.4, 11.5);
        // Num_Siret : 14 cases, y=289.1, h=11.5
        Cases(gfx, Fn, p.Siret?.Replace(" ", ""), SiretAPX0, SiretAPX1, 289.1, 11.5);

        // ── Adresse — 4 champs centrés séparément ────────────────────────────
        // num_VoieAdresse      rect=(108.9, 320.7, 149.9, 332.1)  w=41.0
        // txt_ExtensionAdresse rect=(157.1, 320.7, 197.5, 332.1)  w=40.4
        // txt_TypeVoieAdresse  rect=(206.4, 320.7, 275.4, 332.1)  w=69.0
        // txt_NomVoie          rect=(282.0, 320.7, 558.6, 332.1)  w=276.6
        var (aNum, aExt, aType, aNom) = ParseAdresse(p.AdresseNumVoie);
        // h=8.0 au lieu de 11.4 → centre vertical remonté de ~1.7 pt, texte éloigné du trait
        TxtC(gfx, Fn, aNum,   108.9, 320.7,  41.0, 8.0);
        TxtC(gfx, Ft, aExt,   157.1, 320.7,  40.4, 8.0);
        TxtC(gfx, Ft, aType,  206.4, 320.7,  69.0, 8.0);
        TxtC(gfx, Ft, aNom,   282.0, 320.7, 276.6, 8.0);

        // ── Code postal (cases) / Commune (centré) ────────────────────────────
        Cases(gfx, Fn, p.CodePostal, CpAPX0, CpAPX1, 340.0, 11.4);
        // txt_CommuneAdresse  rect=(192.8, 340.0, 558.6, 351.4) — h=8.0 idem adresse
        TxtC(gfx, Ft, p.Commune, 192.8, 340.0, 365.8, 8.0);

        // ── Certifie (céder / céder pour destruction) ─────────────────────────
        // Céder rect=(184.9, 368.9, 191.9, 375.9) | Destruction rect=(235.9, 368.9, 242.9, 375.9)
        if (p.CederPourDestruction)
            Chk(gfx, Fck, 235.9, 368.9, 7.0, 7.0);
        else
            Chk(gfx, Fck, 184.9, 368.9, 7.0, 7.0);

        // ── Date / Heure de cession — case par case ───────────────────────────
        var (vj, vm, va) = SplitDate(p.DateCession);
        Cases(gfx, Fn, vj, VenteJX0, VenteJX1, 382.5, 11.5);
        Cases(gfx, Fn, vm, VenteMX0, VenteMX1, 382.5, 11.5);
        Cases(gfx, Fn, va, VenteAX0, VenteAX1, 382.5, 11.5);
        Cases(gfx, Fn, p.HeureCession,  HeureX0,  HeureX1,  382.5, 11.5);
        Cases(gfx, Fn, p.MinuteCession, MinuteX0, MinuteX1, 382.5, 11.5);

        // ── Certifie en outre ─────────────────────────────────────────────────
        // ckb_ValidationDéclaration1  rect=(35.9, 418.6, 42.9, 425.6)
        if (p.CertificatSituationAdmin) Chk(gfx, Fck, 35.9, 418.6, 7.0, 7.0);
        // ckb_ValidationDéclaration2  rect=(35.9, 438.3, 42.9, 445.3)
        if (p.PasTransformationNotable) Chk(gfx, Fck, 35.9, 438.3, 7.0, 7.0);
        // ckb_ValidationDéclaration3  rect=(35.9, 456.8, 42.9, 463.8)
        if (p.CessionVhu)
        {
            Chk(gfx, Fck, 35.9, 456.8, 7.0, 7.0);
            // num_Agrément  rect=(140.4, 463.9, 246.9, 475.3)  — centré
            TxtC(gfx, Fn, p.NumeroAgrementVhu, 140.4, 463.9, 106.5, 11.4);
        }

        // ── Fait à / le — ville centrée, date centrée avec espaces ───────────
        // txt_LieuDéclaration1  rect=(63.7, 505.1, 178.5, 516.5)
        TxtC(gfx, Ft, p.LieuFait,               63.7, 505.1, 114.8, 11.4);
        // num_DateDéclaration   rect=(193.9, 505.1, 277.8, 516.5)
        TxtC(gfx, Ft, FormatDate(p.DateFait),   193.9, 505.1,  83.9, 11.4);
    }

    // ── NOUVEAU PROPRIÉTAIRE ─────────────────────────────────────────────────

    private static void RemplirNouveauProprietaire(XGraphics gfx, NouveauProprietaireData p)
    {
        // ── Type de personne ─────────────────────────────────────────────────
        // Physique rect=(35.9, 595.1, 42.9, 602.1) | Morale rect=(35.9, 604.6, 42.9, 611.6)
        if (p.EstPersonneMorale == true)
            Chk(gfx, Fck, 35.9, 604.6, 7.0, 7.0);
        else
            Chk(gfx, Fck, 35.9, 595.1, 7.0, 7.0);

        // Sexe M rect=(261.6, 594.5, 268.6, 601.5) | F rect=(285.7, 594.5, 292.7, 601.5)
        if (p.Sexe == "M") Chk(gfx, Fck, 261.6, 594.5, 7.0, 7.0);
        if (p.Sexe == "F") Chk(gfx, Fck, 285.7, 594.5, 7.0, 7.0);

        // ── Identité / SIRET ─────────────────────────────────────────────────
        // txt_IdentitéAcheteur  rect=(91.0, 620.2, 374.8, 631.6)  — centré
        TxtC(gfx, Ft, p.NomComplet, 91.0, 620.2, 283.8, 11.4);
        // num_SiretAcheteur : 14 cases (mêmes x que AP), y=620.2, h=11.4
        Cases(gfx, Fn, p.Siret?.Replace(" ", ""), SiretAPX0, SiretAPX1, 620.2, 11.4);

        // ── Né(e) le / à ─────────────────────────────────────────────────────
        // Date de naissance : case par case
        var (nj, nm, na) = SplitDate(p.DateNaissance);
        Cases(gfx, Fn, nj, NaissJX0, NaissJX1, 642.3, 11.5);
        Cases(gfx, Fn, nm, NaissMX0, NaissMX1, 642.3, 11.5);
        Cases(gfx, Fn, na, NaissAX0, NaissAX1, 642.3, 11.5);
        // txt_LieuNaissanceAcheteur  rect=(183.2, 642.3, 558.6, 653.8)  — centré
        TxtC(gfx, Ft, p.LieuNaissance, 183.2, 642.3, 375.4, 11.5);

        // ── Adresse — 4 champs centrés séparément ────────────────────────────
        // num_VoieAdresseAcheteur      rect=(108.3, 663.6, 149.3, 675.1)  w=41.0
        // txt_ExtensionAdresseAcheteur rect=(157.6, 663.6, 199.3, 675.1)  w=41.7
        // txt_TypeVoieAdresseAcheteur  rect=(205.2, 663.6, 274.9, 675.1)  w=69.7
        // txt_NomVoieAdresseAcheteur   rect=(280.8, 663.6, 558.6, 675.1)  w=277.8
        var (aNum, aExt, aType, aNom) = ParseAdresse(p.AdresseNumVoie);
        TxtC(gfx, Fn, aNum,  108.3, 663.6,  41.0, 11.5);
        TxtC(gfx, Ft, aExt,  157.6, 663.6,  41.7, 11.5);
        TxtC(gfx, Ft, aType, 205.2, 663.6,  69.7, 11.5);
        TxtC(gfx, Ft, aNom,  280.8, 663.6, 277.8, 11.5);

        // ── Code postal (cases) / Commune (centré) ────────────────────────────
        // num_CodePostalAdresseAcheteur  rect=(108.3, 683.9, 177.3, 695.3)
        Cases(gfx, Fn, p.CodePostal, CpAPX0, CpAPX1, 683.9, 11.4);
        // txt_CommuneAdresseAcheteur     rect=(191.6, 683.9, 558.6, 695.3)
        TxtC(gfx, Ft, p.Commune, 191.6, 683.9, 367.0, 11.4);

        // ── Certifie ─────────────────────────────────────────────────────────
        // ckb_ValidationDéclarationA1  rect=(35.9, 725.3, 42.9, 732.3)
        if (p.CertifieAcquerir)         Chk(gfx, Fck, 35.9, 725.3, 7.0, 7.0);
        // ckb_ValidationDéclarationA2  rect=(35.9, 738.9, 42.9, 745.9)
        if (p.CertifieInformeSituation) Chk(gfx, Fck, 35.9, 738.9, 7.0, 7.0);

        // ── Fait à / le — ville centrée, date centrée avec espaces ───────────
        // txt_LieuDéclaration2  rect=(63.7, 758.8, 179.1, 770.3)
        TxtC(gfx, Ft, p.LieuFait,              63.7, 758.8, 115.4, 11.5);
        // txt_dateDéclaration   rect=(193.3, 758.8, 276.6, 770.3)
        TxtC(gfx, Ft, FormatDate(p.DateFait), 193.3, 758.8,  83.3, 11.5);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static XFont MakeFont(double size) =>
        new("OpenSans", size, XFontStyle.Regular);

    /// <summary>
    /// Dessine chaque caractère du texte centré dans sa case individuelle.
    /// x0s/x1s = bords gauche/droit de chaque case ; y/h = ligne commune.
    /// </summary>
    private static void Cases(XGraphics gfx, double size, string? text,
                              double[] x0s, double[] x1s, double y, double h)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var t = text.Trim();
        for (int i = 0; i < Math.Min(t.Length, x0s.Length); i++)
        {
            var rect = new XRect(x0s[i], y, x1s[i] - x0s[i], h);
            gfx.DrawString(t[i].ToString(), MakeFont(size), XBrushes.Black,
                           rect, XStringFormats.Center);
        }
    }

    /// <summary>Texte aligné à gauche, centré verticalement (champs libres larges).</summary>
    private static void Txt(XGraphics gfx, double size, string? text,
                            double x, double y, double w, double h)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        gfx.DrawString(text.Trim(), MakeFont(size), XBrushes.Black,
                       new XRect(x, y, w, h), XStringFormats.CenterLeft);
    }

    /// <summary>Texte centré horizontalement ET verticalement (D.1/D.2/J.1/D.3).</summary>
    private static void TxtC(XGraphics gfx, double size, string? text,
                             double x, double y, double w, double h)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        gfx.DrawString(text.Trim(), MakeFont(size), XBrushes.Black,
                       new XRect(x, y, w, h), XStringFormats.Center);
    }

    /// <summary>
    /// Dessine un "X" centré dans la case à cocher / bouton radio.
    /// x, y = coin haut-gauche du rect ; w, h = dimensions du rect (≈ 7×7 pt).
    /// </summary>
    private static void Chk(XGraphics gfx, double size,
                            double x, double y, double w, double h)
    {
        var rect = new XRect(x, y, w, h);
        gfx.DrawString("X", MakeFont(size), XBrushes.Black,
                       rect, XStringFormats.Center);
    }

    /// <summary>
    /// Découpe une date "jj/mm/aaaa" en (jour, mois, année).
    /// Retourne ("", "", "") si la date est nulle ou mal formatée.
    /// </summary>
    private static (string jour, string mois, string année) SplitDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date)) return ("", "", "");
        var parts = date.Split('/');
        return parts.Length == 3
            ? (parts[0].Trim(), parts[1].Trim(), parts[2].Trim())
            : ("", "", "");
    }

    /// <summary>
    /// Formate une date "jj/mm/aaaa" → "jj / mm / aaaa" (espaces autour des slashes).
    /// </summary>
    private static string? FormatDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date)) return null;
        return date.Trim().Replace("/", " / ");
    }

    /// <summary>
    /// Découpe une adresse libre en 4 composants CERFA :
    ///   num   — numéro de voie  (ex. "260")
    ///   ext   — extension       (BIS, TER, QUATER…)  — peut être vide
    ///   type  — type de voie    (RUE, AVENUE, BOULEVARD…)
    ///   nom   — nom de la voie  (tout le reste)
    ///
    /// Exemple : "12 BIS RUE DES LILAS"
    ///        → ("12", "BIS", "RUE", "DES LILAS")
    /// </summary>
    private static (string num, string ext, string type, string nom) ParseAdresse(string? adresse)
    {
        if (string.IsNullOrWhiteSpace(adresse)) return ("", "", "", "");

        var words = adresse.Trim().ToUpperInvariant()
                           .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int i = 0;

        // 1. Numéro de voie : premier mot contenant au moins un chiffre
        string num = "";
        if (i < words.Length && words[i].Any(char.IsDigit))
            num = words[i++];

        // 2. Extension : BIS, TER, QUATER, QUINQUIES ou lettre seule A-F
        string[] extensions = ["BIS", "TER", "QUATER", "QUINQUIES", "A", "B", "C", "D", "E", "F"];
        string ext = "";
        if (i < words.Length && extensions.Contains(words[i]))
            ext = words[i++];

        // 3. Type de voie
        string[] types =
        [
            "RUE", "AVENUE", "AV", "BOULEVARD", "BLD", "BD",
            "IMPASSE", "IMP", "ALLÉE", "ALLEE", "CHEMIN", "CHE",
            "ROUTE", "RTE", "PASSAGE", "PLACE", "PL", "SQUARE", "SQ",
            "CITÉ", "CITE", "VOIE", "VILLA", "PARC",
            "ESPLANADE", "PROMENADE", "COURS", "RÉSIDENCE", "RESIDENCE",
            "QUAI", "SENTIER", "VENELLE", "DOMAINE", "LOTISSEMENT",
            "LOT", "HAMEAU", "ZAC", "ZI", "ZA", "PONT", "RUELLE",
            "LIEU-DIT", "LD", "ROND-POINT"
        ];
        string typeVoie = "";
        if (i < words.Length && types.Contains(words[i]))
            typeVoie = words[i++];

        // 4. Reste = nom de la voie
        string nom = string.Join(" ", words.Skip(i));

        return (num, ext, typeVoie, nom);
    }
}
