# 📸 UPGRADE UI - Guía Visual del Inspector

Esta guía te muestra exactamente cómo debe verse el Inspector para cada componente.

---

## 1️⃣ UpgradeCanvas (GameObject Root)

### Canvas Component
```
Canvas:
  ✓ Render Mode: Screen Space - Overlay
  □ Pixel Perfect: (opcional)
  □ Target Display: Display 1
  
Canvas Scaler:
  ✓ UI Scale Mode: Scale With Screen Size
  ✓ Reference Resolution: 
      X: 1920
      Y: 1080
  ✓ Screen Match Mode: Match Width Or Height
  ✓ Match: 0.5
  ✓ Reference Pixels Per Unit: 100
```

---

## 2️⃣ DarkBackground (Image)

### Rect Transform
```
Anchors:
  Min: (0, 0)
  Max: (1, 1)
  
Pivot: (0.5, 0.5)

Position: (0, 0, 0)

Left: 0
Top: 0
Right: 0
Bottom: 0
```

### Image Component
```
Source Image: None (blanco por defecto)
Color: RGBA(0, 0, 0, 180)
Material: None
Raycast Target: ✓ (checked)
```

---

## 3️⃣ UpgradePanel (Image + Components)

### Rect Transform
```
Anchors:
  Min: (0.5, 0.5)
  Max: (0.5, 0.5)
  
Pivot: (0.5, 0.5)

Position: (0, 0, 0)

Width: 1400
Height: 700
```

### Image Component
```
Source Image: None
Color: RGBA(44, 44, 44, 255) // #2C2C2CFF
Material: None
Raycast Target: ✓
```

### Shadow Component (Opcional)
```
Effect Color: RGBA(0, 0, 0, 128)
Effect Distance: (5, -5)
Use Graphic Alpha: ✓
```

### ⭐ Canvas Group Component (CRÍTICO)
```
Alpha: 1
Interactable: ✓
Block Raycasts: ✓
Ignore Parent Groups: □
```

### ⭐ UpgradeUI Script
```
Panel References:
  ✓ Upgrade Panel: [Arrastra UpgradePanel - sí mismo]
  ✓ Canvas Group: [Arrastra el Canvas Group de UpgradePanel]

Card Components:
  ✓ Option1 Card: [Arrastra Option1Card GameObject]
  ✓ Option2 Card: [Arrastra Option2Card GameObject]

Optional Elements:
  ✓ Title Text: [Arrastra TitleText]
  □ Close Button: None (opcional)

Animation Settings:
  ✓ Fade In Duration: 0.3
```

---

## 4️⃣ TitleText (TextMeshProUGUI)

### Rect Transform
```
Anchors:
  Min: (0.5, 1)
  Max: (0.5, 1)
  
Pivot: (0.5, 1)

Position: (0, -50, 0)

Width: 1200
Height: 100
```

### TextMeshProUGUI Component
```
Text Input:
  "¡ELIGE TU MEJORA!"

Main Settings:
  ✓ Font Asset: [Tu fuente TMP]
  □ Material Preset: Default
  ✓ Font Size: 60
  □ Auto Size: unchecked
  
Vertex Color: RGBA(255, 215, 0, 255) // Dorado #FFD700

Alignment:
  Horizontal: Center ⬜
  Vertical: Top ⬜
  
Wrapping: Disabled
Overflow: Overflow

Font Style: Bold
```

---

## 5️⃣ Option1Card (Image + UpgradeCardUI)

### Rect Transform
```
Anchors:
  Min: (0.5, 0.5)
  Max: (0.5, 0.5)
  
Pivot: (0.5, 0.5)

Position: (-380, -50, 0)  // ← Izquierda

Width: 550
Height: 500
```

### Image Component
```
Source Image: None
Color: RGBA(60, 60, 60, 255) // #3C3C3CFF
Material: None
Raycast Target: ✓ (IMPORTANTE para hover)
```

### ⭐ UpgradeCardUI Script
```
UI References:
  ✓ Upgrade Name Text: [Arrastra UpgradeNameText hijo]
  ✓ Upgrade Description Text: [Arrastra UpgradeDescriptionText hijo]
  ✓ Select Button: [Arrastra SelectButton hijo]
  ✓ Upgrade Icon: [Arrastra UpgradeIcon hijo - OPCIONAL]

Visual Settings:
  ✓ Normal Color: RGBA(60, 60, 60, 255) // #3C3C3C
  ✓ Hover Color: RGBA(76, 76, 76, 255) // #4C4C4C
  ✓ Hover Scale: 1.05
  ✓ Animation Duration: 0.2
```

---

## 6️⃣ UpgradeNameText (hijo de Option1Card)

### Rect Transform
```
Anchors:
  Min: (0, 1)
  Max: (1, 1)
  
Pivot: (0.5, 1)

Position: (0, 0, 0)

Left: 0
Right: 0
Top: 0
Height: 80
```

### TextMeshProUGUI Component
```
Text Input:
  "Nombre del Upgrade" (placeholder)

Main Settings:
  ✓ Font Size: 36
  ✓ Auto Size: unchecked
  
Vertex Color: RGBA(255, 215, 0, 255) // Dorado

Alignment:
  Horizontal: Center ⬜
  Vertical: Middle ⬜

Font Style: Bold
```

---

## 7️⃣ UpgradeDescriptionText (hijo de Option1Card)

### Rect Transform
```
Anchors:
  Min: (0, 0)
  Max: (1, 1)
  
Pivot: (0.5, 0.5)

Position: (0, 0, 0)

Left: 30
Right: 30
Top: 100
Bottom: 120
```

### TextMeshProUGUI Component
```
Text Input:
  "Descripción detallada del upgrade que explica
   sus efectos y beneficios..." (placeholder multi-línea)

Main Settings:
  ✓ Font Size: 24
  □ Auto Size: unchecked
  
Vertex Color: RGBA(224, 224, 224, 255) // #E0E0E0

Alignment:
  Horizontal: Left ⬜
  Vertical: Top ⬜

Wrapping: ✓ Enabled
Overflow: Ellipsis (opcional)
```

---

## 8️⃣ SelectButton (Button hijo de Option1Card)

### Rect Transform
```
Anchors:
  Min: (0, 0)
  Max: (1, 0)
  
Pivot: (0.5, 0)

Position: (0, 0, 0)

Left: 40
Right: 40
Bottom: 20
Height: 70
```

### Image Component (del Button)
```
Source Image: UI/Skin/UISprite (default)
Color: RGBA(76, 76, 76, 255) // #4C4C4C
Material: None
Raycast Target: ✓
```

### Button Component
```
Interactable: ✓

Transition: Color Tint
  Target Graphic: [El Image del botón]
  
  Normal Color: RGBA(76, 76, 76, 255)
  Highlighted Color: RGBA(108, 108, 108, 255)
  Pressed Color: RGBA(44, 44, 44, 255)
  Selected Color: RGBA(92, 92, 92, 255)
  Disabled Color: RGBA(128, 128, 128, 128)
  
  Color Multiplier: 1
  Fade Duration: 0.1

Navigation: Automatic
```

### Text (TMP) hijo del Button
```
Text Input: "SELECCIONAR"

Font Size: 32
Vertex Color: RGBA(255, 255, 255, 255) // Blanco

Alignment:
  Horizontal: Center ⬜
  Vertical: Middle ⬜

Font Style: Bold
```

---

## 9️⃣ Option2Card

**CONFIGURACIÓN IDÉNTICA A Option1Card**

**ÚNICA DIFERENCIA:**
```
Rect Transform:
  Position: (+380, -50, 0)  // → Derecha (en vez de -380)
```

Todo lo demás es exactamente igual.

---

## 🔄 Duplicación Rápida

Para crear Option2Card:

1. Selecciona **Option1Card** en Hierarchy
2. Duplica: `Ctrl + D` (Windows) o `Cmd + D` (Mac)
3. Renombra: **Option2Card**
4. Solo cambia **Pos X** a: `+380`
5. ¡Listo! Todas las referencias internas se mantienen.

---

## ✅ Checklist Final de Referencias

Cuando termines, verifica que:

### En UpgradePanel → UpgradeUI:
- [x] Upgrade Panel apunta a sí mismo
- [x] Canvas Group está conectado
- [x] Option1 Card apunta al GameObject correcto
- [x] Option2 Card apunta al GameObject correcto
- [x] Title Text está conectado

### En Option1Card → UpgradeCardUI:
- [x] Upgrade Name Text apunta a su hijo
- [x] Upgrade Description Text apunta a su hijo
- [x] Select Button apunta a su hijo
- [x] (Opcional) Upgrade Icon apunta a su hijo

### En Option2Card → UpgradeCardUI:
- [x] Todas las referencias igual que Option1Card

---

## 🎨 Previsualización de Colores

```
FONDO OSCURO:      ████ RGBA(0, 0, 0, 180)
PANEL PRINCIPAL:   ████ RGBA(44, 44, 44, 255)
TARJETA NORMAL:    ████ RGBA(60, 60, 60, 255)
TARJETA HOVER:     ████ RGBA(76, 76, 76, 255)
TÍTULO DORADO:     ████ RGBA(255, 215, 0, 255)
TEXTO CLARO:       ████ RGBA(224, 224, 224, 255)
BOTÓN NORMAL:      ████ RGBA(76, 76, 76, 255)
BOTÓN HOVER:       ████ RGBA(108, 108, 108, 255)
BOTÓN PRESSED:     ████ RGBA(44, 44, 44, 255)
```

---

## 🛠️ Tip: Verificación Rápida

Si algo no funciona, estos son los errores más comunes:

1. **No se ve la UI:**
   - Canvas Group Alpha debe ser 1
   - Canvas debe estar en Screen Space - Overlay

2. **Botones no responden:**
   - Verifica EventSystem en la escena
   - Image components deben tener "Raycast Target" ✓

3. **Hover no funciona:**
   - La Card necesita un Image component con "Raycast Target" ✓
   - UpgradeCardUI debe estar en la Card, no en el botón

4. **Textos no se ven:**
   - TextMeshPro debe estar importado
   - Alpha del color no debe ser 0
   - Width/Height no deben ser 0

---

**💡 Usa esta guía como referencia mientras configuras el Inspector!**
