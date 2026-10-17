# Xbox Resolution

**Custom Display-Auflösung für den Xbox Dev Mode** – ohne dass die App laufen muss!

## ✨ Features

| Feature | Beschreibung |
|---------|-------------|
| **Presets** | `1920 × 1080` (Standard), `1440 × 1080` |
| **Custom Input** | Eigene Auflösung – egal wie low oder hoch, **maximal 1920 × 1080** |
| **Systemweit** | Gilt für den **gesamten Xbox Dev Mode** |
| **Persistiert** | Bleibt aktiv **auch wenn die App nicht läuft** |

## 🎮 Installation (Xbox Dev Mode)

1. **Entwicklermodus** auf der Xbox aktivieren
2. **Device Portal** öffnen: `https://<XBOX-IP>`
3. Unter **Apps → Apps installieren** die Datei `XboxResolution.appx` hochladen
4. Zertifikat `XboxResolution.pfx` (Passwort: `xboxresolution`) zulassen
5. **Xbox Resolution** starten

## 🕹️ Nutzung

1. App starten
2. **Preset wählen** (1920×1080 oder 1440×1080) **ODER** eigene Werte eingeben
3. **„Anwenden"** klicken
4. Die Auflösung gilt jetzt **für den gesamten Dev Mode** – die App kann geschlossen werden

## 🔧 Technische Details

- **Plattform**: Xbox Series X|S (Dev Mode)
- **Framework**: UWP (C# / XAML)
- **Auflösungsbereich**: 320×240 bis 1920×1080
- **Persistenz**: `LocalSettings` + DevHome Display-Override

## 🛠️ Building from Source

Der Build läuft automatisch über **GitHub Actions**:

1. Push auf `main` / `master`
2. Workflow **"Build XboxResolution UWP (Xbox)"** startet
3. Artifact **`XboxResolution-Xbox`** herunterladen (enthält `.appx` + `.pfx`)

## 📦 Projektstruktur

```
XboxResolution/
├── .github/workflows/build.yml      # GitHub Actions Build
├── src/XboxResolution/
│   ├── App.xaml / App.xaml.cs        # App-Einstieg
│   ├── MainPage.xaml / .cs           # UI: Presets + Custom Input
│   ├── Services/DisplayService.cs    # Resolution-Setter (systemweit + persistiert)
│   └── Package.appxmanifest
└── README.md
```

## ⚠️ Hinweise

- **Maximale Auflösung**: 1920 × 1080 (alles darüber wird abgelehnt)
- Die Einstellung wird für den **Dev Mode** gesetzt und überlebt App-Neustarts
- **Zurücksetzen**: Button „Auf Standard zurücksetzen" in der App