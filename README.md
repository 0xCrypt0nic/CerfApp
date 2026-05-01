# CessionApp

A mobile application for **vehicle transfer certificates** (CERFA 15776\*02), built for Android and iOS.

---

## What does it do?

CessionApp guides users step by step through filling in the information required for a vehicle transfer between private individuals or with a professional. Once all the data has been entered, the app automatically generates a **print-ready PDF**, with every field positioned pixel-perfectly on the official form.

### Key features

- **OCR scanning** — driving licence and ID cards scanned in real time via the camera, fields filled automatically
- **Step-by-step flow** across 4 screens: vehicle → previous owner → new owner → summary
- **PDF generation** of the completed CERFA 15776\*02, with cell-by-cell placement (licence plate, VIN, dates, SIRET…)
- **Editable summary** — every field can be corrected before the final PDF is generated
- Support for **individuals and legal entities** on both sides of the transfer
- Support for **transfer for destruction** (end-of-life vehicle scheme) with approval number
- Management of the **administrative status certificate** (present / absent / reason)

---

## Tech stack

| Layer | Technology |
|---|---|
| Framework | .NET 10 — .NET MAUI (Android & iOS) |
| Live OCR | CommunityToolkit.Maui.Camera + Google ML Kit |
| PDF generation | PdfSharpCore |
| Language | C# 13 |

---

## Project structure

```
CessionApp/
├── Models/                 # CessionData, VehiculeData, ProprietaireData
├── Services/
│   ├── CerfaGenerator      # Pixel-perfect PDF generation
│   └── OcrService          # Text recognition (ML Kit)
├── Controls/
│   └── HeaderView          # Custom navigation bar (iOS & Android)
├── Resources/
│   ├── Raw/vide.pdf        # Blank CERFA template
│   └── Fonts/              # OpenSans, FontAwesome
├── MainPage                # Step 1 — Vehicle
├── AncienProprietairePage  # Step 2 — Previous owner
├── NouveauProprietairePage # Step 3 — New owner
├── RecapitulatifPage       # Step 4 — Summary & PDF generation
└── ParametresPage          # Settings (coming soon)
```

---

## Built with Claude Code

This project was entirely built through **AI-assisted development** using [Claude Code](https://claude.ai/claude-code) by Anthropic — from the initial architecture to pixel-perfect PDF field placement, OCR regex patterns, CERFA edge case handling, and UI polish.

Not a single line of code was written by hand.

---

## Status

🚧 Work in progress — personal / experimental use.
