# MachBlockKaputt (AlleywayMonoGame)

Ein modernes Breakout/Alleyway-Spiel mit MonoGame (DesktopGL) - **Refactored nach Clean Code-Prinzipien**

## 🎮 Features

- 🎯 **10 progressive Level** mit 4 verschiedenen Patterns
- ⚡ **Shoot-Modus** mit Raketen-Projektilen
- 🛒 **Shop-System** mit Upgrades (Speed, Extra Balls, Shoot Mode)
- 💥 **Particle Effects** und Explosionen
- 🎵 **Prozedural generierte Sounds** (keine externen Audio-Dateien)
- ❤️ **Leben-System** mit Hearts
- ⏱️ **Timer und Time-Bonus**
- 🏗️ **Saubere, modulare Architektur**

## 📋 Voraussetzungen

- .NET 10.0 SDK (oder neuer)
- MonoGame 3.8.4.1 DesktopGL

## 🚀 Build & Ausführung

```powershell
# Projekt builden
dotnet build AlleywayMonoGame.csproj

# Spiel starten
dotnet run --project AlleywayMonoGame.csproj
```

## 🎮 Steuerung

- **Pfeiltasten Links/Rechts** oder **A/D**: Paddle bewegen
- **Leertaste**: 
  - Ball starten (zu Beginn)
  - Rakete abfeuern (im Shoot-Modus)
- **Escape**: Spiel beenden
- **P**: Cheat - Level sofort gewinnen (Debug)

## 🏗️ Projekt-Architektur

Das Projekt folgt **Clean Code-Prinzipien** und ist in modulare Komponenten aufgeteilt:

```
MachBlockKaputt/
├── Models/         # Datenmodelle (Ball, Particle, FloatingText)
├── Entities/       # Spielobjekte (Paddle, Brick, Projectile)
├── Services/       # Business Logic (Audio, Score, Shop)
├── Systems/        # Spielsysteme (Collision, Particle, Level)
├── Core/           # Infrastruktur (Constants, StateManager, TextureFactory)
└── Game1.cs        # Hauptspiel-Loop
```

**Details siehe:** [REFACTORING.md](REFACTORING.md)

## 📦 Komponenten

### Services
- **AudioService**: Sound-Generierung & Wiedergabe
- **ScoreService**: Score, Lives, Timer-Management
- **ShopService**: Shop-Logik & Upgrades

### Systems
- **CollisionSystem**: Kollisionserkennung
- **ParticleSystem**: Partikel-Effekte
- **LevelSystem**: Level-Generierung
- **FloatingTextSystem**: Text-Animationen

### Entities
- **Paddle**: Spieler-Steuerung
- **Brick**: Zerstörbare Blöcke (3 Typen)
- **Projectile**: Raketen-Geschosse

## 🎨 Besonderheiten

- ✅ **Keine externen Assets**: Alle Texturen und Sounds werden prozedural zur Laufzeit generiert
- ✅ **3D-Effekte**: Ball mit Lighting, Paddle mit Metallic-Effekt
- ✅ **Responsive UI**: Grid-basiertes Layout-System
- ✅ **Modulare Architektur**: Leicht erweiterbar und wartbar

## 📄 Dateien

- `Program.cs` - Entry Point
- `Game1.cs` - Hauptspiel-Loop (2000+ Zeilen)
- `AlleywayMonoGame.csproj` - Projekt-Datei
- `Content/` - Content Pipeline (DefaultFont.spritefont)
- `REFACTORING.md` - Architektur-Dokumentation

## 🔧 Entwicklung

Das Projekt ist bereit für:
- ✅ Unit-Tests
- ✅ Neue Features
- ✅ Team-Entwicklung
- ✅ Erweiterungen

Siehe [REFACTORING.md](REFACTORING.md) für Details zur neuen Architektur.

## 📜 Lizenz

MIT
