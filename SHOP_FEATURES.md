# Shop Enhancement Features

## Implementierte Features

### 1. Reroll-Funktion
- **Kosten**: $5
- **Funktion**: Generiert neue zufällige Shop-Items
- **Button**: Zentral unter den Shop-Items mit "↻ REROLL $5" Text
- **Visuell**: Lila/violette Farbe (150, 100, 200)
- **Sound**: PowerUp-Sound beim Reroll

### 2. Item-Icons
Jedes Item hat nun ein passendes Unicode-Icon:
- **Speed Upgrade** `→` - Pfeil nach rechts (Bewegung)
- **Extra Ball** `●` - Gefüllter Kreis (Ball)
- **Shoot Mode** `↑` - Pfeil nach oben (Schießen)
- **Paddle Size** `═` - Doppellinie (Breite)

### 3. Color Coding
Jedes Item hat eine eindeutige Farbe basierend auf seinem Typ:
- **Speed Upgrade**: Blau (100, 200, 255) - Bewegung
- **Extra Ball**: Gold (255, 215, 0) - Wertvoll
- **Shoot Mode**: Rot (255, 100, 100) - Kampf
- **Paddle Size**: Grün (150, 255, 150) - Größe

Die Button-Hintergrundfarbe wird mit der Item-Farbe gemischt (30% blend)

### 4. Hover-Tooltips
Beim Hovern über ein Item erscheint eine Tooltip-Box mit:
- **Rahmen**: In der Item-Farbe (Pixel-Art Stil)
- **Beschreibung**: Mehrzeiliger Text mit genauem Effekt
- **Position**: Zentriert über dem Shop
- **Design**: Dunkler Hintergrund (10, 10, 30), transparentes Overlay

#### Tooltip-Texte:
```
Speed Upgrade:
  "Increases paddle
   movement speed by 3%"

Extra Ball:
  "Adds one extra ball
   at level start"

Shoot Mode:
  "Start next level
   with shoot mode active"

Paddle Size:
  "Increases paddle
   width by 4%"
```

## Technische Details

### ShopService.cs - Neue Methoden:
```csharp
public bool CanAffordReroll()        // Prüft ob Reroll möglich
public bool Reroll()                 // Führt Reroll durch
public string GetItemIcon(ShopItem)  // Icon für Item
public string GetItemDescription()   // Beschreibung für Tooltip
public Color GetItemColor(ShopItem)  // Farbe für Item
```

### UIManager.cs - Neue Properties:
```csharp
public Rectangle RerollButton        // Reroll-Button Hitbox
public bool RerollButtonHovered      // Hover-Status
public int HoveredShopItem           // Aktuell gehovertes Item (-1 = keins)
```

### InputHandler.cs - Erweitert:
```csharp
ShopInputResult.RerollClicked        // Neues Flag für Reroll
```

### DialogRenderer.cs - Visuelles:
- Shop-Box erhöht: 190 → 240 Höhe
- Main-Box erhöht: 550 → 650 Höhe
- Main-Box Position: Y=50 → Y=30 (höher platziert)
- Icon + Text + Cost Layout für jedes Item
- Reroll-Button unter Items
- Tooltip-System mit Pixel-Art Rahmen

## Verwendung

1. **Shop öffnet sich nach Level-Complete**
2. **Items mit Icons und Farben werden angezeigt**
3. **Hovern über Item zeigt Tooltip mit Beschreibung**
4. **Klick auf Item kauft es (wenn genug Geld)**
5. **Klick auf Reroll-Button für $5 generiert neue Items**
6. **"NEXT LEVEL" startet nächstes Level**

## Design-Philosophie

- **Pixel-Art Stil**: Alle neuen Elemente folgen dem bestehenden Pixel-Design
- **Farbcodierung**: Intuitive Zuordnung (Blau=Bewegung, Gold=Wertvoll, etc.)
- **Klare Informationen**: Tooltips geben genaue Effekt-Beschreibungen
- **Strategische Tiefe**: Reroll ermöglicht bessere Item-Auswahl
- **Visuelle Hierarchie**: Icons, Farben und Layout lenken Aufmerksamkeit
