using CerfApp.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace CerfApp.Services;

public static class Cerfa13750Generator
{
    private const double Ft  = 8.5;
    private const double Fn  = 8.0;
    private const double Fck = 8.0;

    public static async Task<string> GenererAsync(Cerfa13750Data data)
    {
        using var templateStream = await FileSystem.OpenAppPackageFileAsync("cerfa_13750-07.pdf");
        using var ms = new MemoryStream();
        await templateStream.CopyToAsync(ms);
        ms.Position = 0;

        using var doc = PdfReader.Open(ms, PdfDocumentOpenMode.Modify);
        using var gfx = XGraphics.FromPdfPage(doc.Pages[0]);

        RemplirPage(gfx, data);

        var outputPath = Path.Combine(FileSystem.CacheDirectory, "cerfa_13750_genere.pdf");
        doc.Save(outputPath);
        return outputPath;
    }

    private static void RemplirPage(XGraphics gfx, Cerfa13750Data d)
    {
        RemplirTypeDemande(gfx, d.TypeDemande);
        RemplirVehicule(gfx, d.Vehicule, d.SituationLocative);
        RemplirTitulaire(gfx, d.Titulaire);
        if (d.SituationLocative != SituationLocative.Non)
            RemplirLoueur(gfx, d.Loueur);
        if (d.SituationLocative == SituationLocative.LongueDuree ||
            d.SituationLocative == SituationLocative.CreditBail)
            RemplirLocataire(gfx, d.Locataire);
        RemplirOpposition(gfx, d);
        PlacerSignature(gfx, d.SignatureTitulaire,  20.0, 730.0, 140.0, 40.0);
        PlacerSignature(gfx, d.SignatureLoueur,     165.0, 730.0, 130.0, 40.0);
        PlacerSignature(gfx, d.SignatureLocataire,  300.0, 730.0, 130.0, 40.0);
    }

    // ── TYPE DE DEMANDE ──────────────────────────────────────────────────────

    private static void RemplirTypeDemande(XGraphics gfx, TypeDemande13750 type)
    {
        switch (type)
        {
            case TypeDemande13750.Certificat:
                Chk(gfx, Fck, 147.75, 74.05, 10.57, 10.01); break;
            case TypeDemande13750.Duplicata:
                Chk(gfx, Fck, 208.58, 74.05, 10.56, 10.66); break;
            case TypeDemande13750.Correction:
                Chk(gfx, Fck, 270.60, 74.05, 10.56, 10.01); break;
            case TypeDemande13750.ChangementDomicile:
                Chk(gfx, Fck, 337.50, 74.05, 10.56, 10.01); break;
            case TypeDemande13750.ChangementEtatCivil:
                Chk(gfx, Fck, 80.75, 89.23, 10.25, 10.00); break;
            case TypeDemande13750.ChangementTechnique:
                Chk(gfx, Fck, 280.31, 89.23, 10.24, 10.00); break;
        }
    }

    // ── VÉHICULE ─────────────────────────────────────────────────────────────

    private static void RemplirVehicule(XGraphics gfx, Cerfa13750VehiculeData v, SituationLocative loc)
    {
        Txt(gfx, Fn, v.Immatriculation, 34.25, 121.76, 113.78, 18.00);

        var (ej, em, ea) = Split(v.DateEntree);
        Txt(gfx, Fn, ej, 165.02, 121.76,  28.54, 18.00);
        Txt(gfx, Fn, em, 196.28, 121.76,  28.44, 18.00);
        Txt(gfx, Fn, ea, 227.72, 121.76,  57.02, 18.00);

        var (cj, cm, ca) = Split(v.DateCertificat);
        Txt(gfx, Fn, cj, 303.58, 121.76,  27.30, 18.00);
        Txt(gfx, Fn, cm, 334.27, 121.76,  28.43, 18.00);
        Txt(gfx, Fn, ca, 365.01, 121.76,  58.17, 18.00);

        var (mj, mm, ma) = Split(v.DateMiseCirculation);
        Txt(gfx, Fn, mj, 441.56, 121.76,  28.19, 18.00);
        Txt(gfx, Fn, mm, 472.18, 121.76,  28.65, 18.00);
        Txt(gfx, Fn, ma, 504.21, 121.76,  56.28, 18.00);

        Txt(gfx, Ft, v.NumeroFormule,       34.65, 154.41, 156.62, 15.46);
        Txt(gfx, Ft, v.Marque,              33.68, 177.67, 175.32, 15.83);
        Txt(gfx, Ft, v.Modele,             222.84, 177.67,  96.04, 15.83);
        Txt(gfx, Ft, v.TypeMine,            33.68, 198.29, 285.20, 17.19);
        Txt(gfx, Ft, v.Serie,               33.18, 219.21, 176.00, 17.05);
        Txt(gfx, Ft, v.Genre,              222.52, 219.21,  96.04, 17.05);
        Txt(gfx, Ft, v.NumeroExploitation,  34.39, 241.30, 174.57, 15.66);

        switch (v.Couleur)
        {
            case CouleurVehicule.Noire:  Chk(gfx, Fck, 420.72, 206.22, 7.22, 6.73); break;
            case CouleurVehicule.Jaune:  Chk(gfx, Fck, 466.54, 206.54, 7.22, 6.74); break;
            case CouleurVehicule.Marron: Chk(gfx, Fck, 420.72, 219.49, 7.22, 6.73); break;
            case CouleurVehicule.Vert:   Chk(gfx, Fck, 466.54, 219.99, 7.22, 6.74); break;
            case CouleurVehicule.Rouge:  Chk(gfx, Fck, 420.56, 233.16, 7.22, 6.73); break;
            case CouleurVehicule.Bleu:   Chk(gfx, Fck, 466.54, 233.16, 7.22, 6.73); break;
            case CouleurVehicule.Orange: Chk(gfx, Fck, 420.72, 246.99, 7.22, 6.73); break;
            case CouleurVehicule.Beige:  Chk(gfx, Fck, 466.54, 246.66, 7.22, 6.73); break;
        }

        if (v.Teinte == TeinteVehicule.Clair)
            Chk(gfx, Fck, 374.01, 216.14, 6.07, 6.45);
        else if (v.Teinte == TeinteVehicule.Fonce)
            Chk(gfx, Fck, 374.17, 240.88, 5.75, 6.13);

        switch (loc)
        {
            case SituationLocative.LongueDuree:
                Chk(gfx, Fck, 306.89, 267.87, 8.21, 7.55); break;
            case SituationLocative.CourteDuree:
                Chk(gfx, Fck, 407.64, 267.55, 8.04, 7.87); break;
            case SituationLocative.CreditBail:
                Chk(gfx, Fck, 507.24, 267.71, 7.87, 8.04); break;
        }
    }

    // ── TITULAIRE ────────────────────────────────────────────────────────────

    private static void RemplirTitulaire(XGraphics gfx, Cerfa13750TitulaireData t)
    {
        if (t.EstPersonneMorale)
            Chk(gfx, Fck, 365.43, 296.75, 8.04, 7.82);
        else
        {
            Chk(gfx, Fck, 207.27, 296.97, 7.17, 7.39);
            if (t.Sexe == "M") Chk(gfx, Fck, 260.31, 296.53, 8.04, 8.04);
            if (t.Sexe == "F") Chk(gfx, Fck, 284.19, 296.75, 8.04, 7.61);
        }

        Txt(gfx, Ft, t.Siret,     402.87, 290.14, 158.08, 16.39);
        Txt(gfx, Ft, t.NomPrenom,  76.00, 304.89, 309.00, 19.00);
        Txt(gfx, Ft, t.NomUsage,  391.00, 304.89, 170.00, 19.00);

        if (!t.EstPersonneMorale)
        {
            var (nj, nm, na) = Split(t.DateNaissance);
            Txt(gfx, Fn, nj,              56.59, 327.68,  22.83, 13.94);
            Txt(gfx, Fn, nm,              82.63, 327.68,  22.52, 13.94);
            Txt(gfx, Fn, na,             107.79, 327.68,  45.87, 13.94);
            Txt(gfx, Ft, t.VilleNaissance, 172.00, 327.89, 198.00, 14.00);
            Txt(gfx, Fn, t.DepNaissance,  378.26, 327.68,  34.30, 13.94);
            Txt(gfx, Ft, t.PaysNaissance, 421.00, 327.89, 140.00, 14.00);
        }

        Txt(gfx, Ft, t.Etage,     84.00, 345.89, 236.00, 14.00);
        Txt(gfx, Ft, t.Immeuble,  326.00, 345.89, 236.00, 14.00);
        Txt(gfx, Fn, t.NumVoie,    84.00, 362.89,  45.00, 13.00);
        Txt(gfx, Ft, t.Extension,  133.00, 362.89,  50.00, 13.00);
        Txt(gfx, Ft, t.TypeVoie,   189.00, 362.89,  77.00, 13.00);
        Txt(gfx, Ft, t.NomVoie,    272.00, 362.89, 289.00, 13.00);
        Txt(gfx, Ft, t.LieuDit,    83.00, 378.89, 260.00, 12.00);
        Txt(gfx, Fn, t.Telephone,  347.00, 378.89, 213.00, 12.00);
        Txt(gfx, Fn, t.CodePostal,  83.40, 394.66,  58.65, 11.81);
        Txt(gfx, Ft, t.Commune,    149.00, 393.89, 195.00, 12.00);
        Txt(gfx, Fn, t.Mail,       347.00, 393.89, 213.00, 12.00);

        Txt(gfx, Fn, t.NbCotitulaires,   450.89, 408.57,  28.06, 13.87);
        Txt(gfx, Ft, t.CotitulaireNom,    82.00, 418.89, 162.00, 15.00);
        Txt(gfx, Fn, t.CotitulaireSiret,  253.00, 418.89, 139.00, 15.00);

        var (sj, sm, sa) = Split(t.DateSignature);
        Txt(gfx, Ft, t.VilleSignature,  54.58, 713.62,  47.73, 12.25);
        Txt(gfx, Fn, sj,               109.73, 713.62,  12.82, 12.25);
        Txt(gfx, Fn, sm,               122.83, 713.62,  11.95, 12.25);
        Txt(gfx, Fn, sa,               136.06, 713.62,  24.47, 12.25);
    }

    // ── LOUEUR ───────────────────────────────────────────────────────────────

    private static void RemplirLoueur(XGraphics gfx, Cerfa13750PersonneData l)
    {
        if (l.EstPersonneMorale)
            Chk(gfx, Fck, 365.78, 464.51, 7.60, 7.71);
        else
        {
            Chk(gfx, Fck, 206.83, 464.29, 7.73, 7.92);
            if (l.Sexe == "M") Chk(gfx, Fck, 260.21, 464.29, 8.48, 7.93);
            if (l.Sexe == "F") Chk(gfx, Fck, 284.58, 464.29, 7.39, 7.93);
        }

        Txt(gfx, Ft, l.Siret,     403.43, 458.08, 159.40, 12.28);
        Txt(gfx, Ft, l.NomPrenom,  83.15, 471.89, 307.93, 12.97);
        Txt(gfx, Ft, l.NomUsage,  398.52, 471.45, 164.32, 13.41);
        Txt(gfx, Ft, l.Etage,      83.35, 489.71, 235.89, 13.60);
        Txt(gfx, Ft, l.Immeuble,  326.32, 489.71, 235.90, 13.60);
        Txt(gfx, Fn, l.NumVoie,    83.60, 507.50,  43.31, 14.26);
        Txt(gfx, Ft, l.Extension,  132.91, 507.50,  51.40, 14.26);
        Txt(gfx, Ft, l.TypeVoie,   189.44, 507.50,  75.90, 14.26);
        Txt(gfx, Ft, l.NomVoie,    272.60, 507.50, 290.24, 14.26);
        Txt(gfx, Ft, l.LieuDit,    83.82, 526.87, 259.47, 12.87);
        Txt(gfx, Fn, l.Telephone,  347.35, 526.87, 212.82, 12.87);
        Txt(gfx, Fn, l.CodePostal,  83.12, 543.13,  56.47, 14.55);
        Txt(gfx, Ft, l.Commune,    149.12, 543.13, 193.76, 14.55);
        Txt(gfx, Fn, l.Mail,       348.03, 543.13, 213.26, 14.55);

        var (sj, sm, sa) = Split(l.DateSignature);
        Txt(gfx, Ft, l.VilleSignature, 187.85, 715.62,  48.46, 12.25);
        Txt(gfx, Fn, sj,               244.69, 715.62,  12.81, 12.25);
        Txt(gfx, Fn, sm,               258.78, 715.62,  11.95, 12.25);
        Txt(gfx, Fn, sa,               272.06, 715.62,  25.78, 12.25);
    }

    // ── LOCATAIRE ────────────────────────────────────────────────────────────

    private static void RemplirLocataire(XGraphics gfx, Cerfa13750PersonneData l)
    {
        if (l.EstPersonneMorale)
            Chk(gfx, Fck, 366.14, 587.57, 7.71, 7.47);
        else
        {
            Chk(gfx, Fck, 207.60, 587.41, 7.38, 8.12);
            if (l.Sexe == "M") Chk(gfx, Fck, 261.04, 587.57, 7.55, 7.96);
            if (l.Sexe == "F") Chk(gfx, Fck, 284.82, 587.41, 8.04, 8.12);
        }

        Txt(gfx, Ft, l.Siret,     403.65, 580.64, 158.79, 13.78);
        Txt(gfx, Ft, l.NomPrenom,  84.17, 594.97, 309.49, 13.14);
        Txt(gfx, Ft, l.NomUsage,  399.48, 595.97, 164.31, 13.14);
        Txt(gfx, Ft, l.Etage,      84.10, 613.31, 237.53, 14.55);
        Txt(gfx, Ft, l.Immeuble,  326.74, 613.31, 235.89, 14.55);
        Txt(gfx, Fn, l.NumVoie,    84.22, 632.44,  43.31, 12.50);
        Txt(gfx, Ft, l.Extension,  133.37, 632.44,  51.39, 12.50);
        Txt(gfx, Ft, l.TypeVoie,   189.39, 632.44,  78.65, 12.50);
        Txt(gfx, Ft, l.NomVoie,    273.48, 632.44, 289.33, 12.50);
        Txt(gfx, Ft, l.LieuDit,    83.73, 650.30, 259.30, 12.81);
        Txt(gfx, Fn, l.Telephone,  348.11, 650.30, 211.85, 12.81);
        Txt(gfx, Fn, l.CodePostal,  83.88, 668.28,  57.47, 12.65);
        Txt(gfx, Ft, l.Commune,    148.27, 668.28, 194.36, 12.65);
        Txt(gfx, Fn, l.Mail,       347.46, 668.28, 214.11, 12.65);

        var (sj, sm, sa) = Split(l.DateSignature);
        Txt(gfx, Ft, l.VilleSignature, 322.25, 715.62,  47.84, 12.25);
        Txt(gfx, Fn, sj,               379.48, 715.62,  12.81, 12.25);
        Txt(gfx, Fn, sm,               394.07, 715.62,  11.95, 12.25);
        Txt(gfx, Fn, sa,               407.10, 715.62,  25.12, 12.25);
    }

    // ── OPPOSITION ───────────────────────────────────────────────────────────

    private static void RemplirOpposition(XGraphics gfx, Cerfa13750Data d)
    {
        if (d.Titulaire.Opposition)  Chk(gfx, Fck, 163.90, 780.03, 6.31, 6.07);
        if (d.Loueur.Opposition)     Chk(gfx, Fck, 239.21, 780.03, 6.30, 6.07);
        if (d.Locataire.Opposition)  Chk(gfx, Fck, 321.34, 780.03, 6.31, 6.07);
    }

    // ── SIGNATURE ────────────────────────────────────────────────────────────

    private static void PlacerSignature(XGraphics gfx, byte[]? png,
                                        double x, double y, double w, double h)
    {
        if (png == null || png.Length == 0) return;
        using var image = XImage.FromStream(() => new MemoryStream(png));
        gfx.DrawImage(image, x, y, w, h);
    }

    // ── HELPERS ──────────────────────────────────────────────────────────────

    private static XFont MakeFont(double size) =>
        new("OpenSans", size, XFontStyle.Regular);

    private static void Txt(XGraphics gfx, double size, string? text,
                            double x, double y, double w, double h)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        gfx.DrawString(text.Trim(), MakeFont(size), XBrushes.Black,
                       new XRect(x, y, w, h), XStringFormats.CenterLeft);
    }

    private static void Chk(XGraphics gfx, double size,
                            double x, double y, double w, double h)
    {
        gfx.DrawString("X", MakeFont(size), XBrushes.Black,
                       new XRect(x, y, w, h), XStringFormats.Center);
    }

    private static (string j, string m, string a) Split(string? date)
    {
        if (string.IsNullOrWhiteSpace(date)) return ("", "", "");
        var p = date.Split('/');
        return p.Length == 3 ? (p[0].Trim(), p[1].Trim(), p[2].Trim()) : ("", "", "");
    }
}
