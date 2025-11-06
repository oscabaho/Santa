# 🎯 UPGRADE UI - Guía Rápida de Referencia

## 📊 Jerarquía Completa del Prefab

```
UpgradeCanvas (Canvas)
  ├── DarkBackground (Image) - Fondo oscuro transparente
  └── UpgradePanel (Image + CanvasGroup + UpgradeUI.cs)
      ├── TitleText (TextMeshPro) - "¡ELIGE TU MEJORA!"
      ├── Option1Card (Image + UpgradeCardUI.cs)
      │   ├── UpgradeNameText (TextMeshPro)
      │   ├── UpgradeDescriptionText (TextMeshPro)
      │   ├── UpgradeIcon (Image) [Opcional]
      │   └── SelectButton (Button)
      │       └── Text (TMP) - "SELECCIONAR"
      └── Option2Card (Image + UpgradeCardUI.cs)
          ├── UpgradeNameText (TextMeshPro)
          ├── UpgradeDescriptionText (TextMeshPro)
          ├── UpgradeIcon (Image) [Opcional]
          └── SelectButton (Button)
              └── Text (TMP) - "SELECCIONAR"
```

---

## 🔧 Configuraciones Críticas

### Canvas
- **Render Mode:** Screen Space - Overlay
- **Reference Resolution:** 1920x1080
- **Match:** 0.5

### UpgradePanel
- **DEBE tener:** `CanvasGroup` component (para fade in/out)
- **DEBE tener:** `UpgradeUI` script
- **Size:** 1400x700

### Cada Card (Option1Card, Option2Card)
- **DEBE tener:** `UpgradeCardUI` script
- **DEBE tener:** `Image` component (para hover effects)
- **Size:** 550x500
- **Spacing:** 760px entre centros (±380 desde el centro del panel)

---

## 📝 Referencias del Inspector

### UpgradeUI Script (en UpgradePanel)
```
✓ Upgrade Panel → UpgradePanel (sí mismo)
✓ Canvas Group → CanvasGroup del UpgradePanel
✓ Option1 Card → Option1Card
✓ Option2 Card → Option2Card
✓ Title Text → TitleText
✓ Fade In Duration → 0.3
```

### UpgradeCardUI Script (en cada Card)
```
✓ Upgrade Name Text → UpgradeNameText (hijo)
✓ Upgrade Description Text → UpgradeDescriptionText (hijo)
✓ Select Button → SelectButton (hijo)
✓ Upgrade Icon → UpgradeIcon (hijo) [Opcional]
✓ Normal Color → #3C3C3C
✓ Hover Color → #4C4C4C
✓ Hover Scale → 1.05
✓ Animation Duration → 0.2
```

---

## 🎨 Paleta de Colores Recomendada

| Elemento | Color Hex | RGB | Uso |
|----------|-----------|-----|-----|
| Dark Background | `#000000B4` | 0,0,0,180 | Fondo semitransparente |
| Panel Background | `#2C2C2C` | 44,44,44 | Fondo del panel principal |
| Card Normal | `#3C3C3C` | 60,60,60 | Cards en estado normal |
| Card Hover | `#4C4C4C` | 76,76,76 | Cards en hover |
| Title Gold | `#FFD700` | 255,215,0 | Texto del título |
| Name Gold | `#FFD700` | 255,215,0 | Nombre del upgrade |
| Description | `#E0E0E0` | 224,224,224 | Texto de descripción |
| Button Normal | `#4C4C4C` | 76,76,76 | Botón normal |
| Button Hover | `#6C6C6C` | 108,108,108 | Botón hover |
| Button Pressed | `#2C2C2C` | 44,44,44 | Botón presionado |

---

## ⚡ Testing Rápido

### Setup Testing (5 minutos)
1. Crea 2 ScriptableObjects: `Assets/Scriptable Objects/Test_Upgrade1.asset`
   - Click derecho → `Create` → `Santa` → `Ability Upgrade`
   - Llenar: Name, Description
2. Añade `UpgradeUITester.cs` a un GameObject en la escena
3. Arrastra los 2 upgrades de prueba al Inspector
4. Play Mode → Presiona **T** para mostrar la UI

### Checklist de Pruebas
- [ ] Panel aparece con fade-in suave
- [ ] Tarjetas muestran nombre y descripción
- [ ] Hover cambia color y escala de las tarjetas
- [ ] Botones son clickeables
- [ ] Al elegir, se aplica el upgrade y se cierra el panel
- [ ] No hay errores en la consola

---

## 🐛 Errores Comunes y Soluciones

### ❌ "NullReferenceException: Object reference not set..."
**Causa:** Referencias no asignadas en el Inspector  
**Solución:** Verifica que TODAS las referencias del Inspector estén conectadas

### ❌ "The UpgradeUI is not showing"
**Causa:** Canvas no está en Overlay o está desactivado  
**Solución:** 
1. Canvas → Render Mode = Screen Space - Overlay
2. UpgradePanel → CanvasGroup → Alpha = 1, Interactable = true

### ❌ "Buttons are not responding"
**Causa:** Falta EventSystem  
**Solución:** Hierarchy → Click derecho → UI → Event System

### ❌ "Text is not visible"
**Causa:** TextMeshPro no importado  
**Solución:** Window → TextMeshPro → Import TMP Essential Resources

### ❌ "Cards don't have hover effect"
**Causa:** EventSystem no detecta la tarjeta  
**Solución:** Asegúrate de que la Card tenga un `Image` component (para raycast)

---

## 🚀 Flujo de Integración

```
1. Combate termina (victoria)
         ↓
2. UpgradeManager.PresentUpgradeOptions()
         ↓
3. IUpgradeUI.ShowUpgrades(upgrade1, upgrade2)
         ↓
4. UpgradeUI muestra el panel con fade-in
         ↓
5. Usuario elige un upgrade
         ↓
6. UpgradeCardUI dispara evento OnUpgradeSelected
         ↓
7. UpgradeUI.OnUpgradeChosen()
         ↓
8. IUpgradeService.ApplyUpgrade()
         ↓
9. ILevelService.LiberateCurrentLevel()
         ↓
10. ICombatTransitionService.EndCombat()
         ↓
11. ILevelService.AdvanceToNextLevel()
         ↓
12. UpgradeUI se oculta con fade-out
```

---

## 🎯 Extensiones Futuras Sugeridas

### Fácil (1-2 horas)
- [ ] Sonidos para show/hide/select
- [ ] Partículas al seleccionar upgrade
- [ ] Botón de "Cerrar" o "Skip"

### Medio (3-5 horas)
- [ ] Sistema de raridad con colores
- [ ] Preview de stats (antes/después)
- [ ] Animación de entrada de las tarjetas (stagger)
- [ ] Sistema de 3+ opciones

### Avanzado (1+ días)
- [ ] Sistema de Reroll (gastar moneda para nuevas opciones)
- [ ] Historial de upgrades obtenidos en la run
- [ ] Tooltips con más información
- [ ] Sistema de synergias (combinaciones de upgrades)

---

## 📱 Responsive Design

El sistema actual funciona bien en:
- ✅ 1920x1080 (Full HD)
- ✅ 1280x720 (HD)
- ✅ 2560x1440 (2K)

Para otras resoluciones, ajusta:
- **Match slider** en Canvas Scaler
- **Min/Max font sizes** en TextMeshPro

---

## 📄 Archivos Creados

```
✓ Assets/Scripts/Upgrades/UpgradeUI.cs (refactorizado)
✓ Assets/Scripts/Upgrades/Components/UpgradeCardUI.cs (nuevo)
✓ Assets/Scripts/Upgrades/UpgradeUITester.cs (testing)
✓ Assets/Prefabs/UI/UpgradeUI.prefab (tu prefab final)
✓ UPGRADE_UI_SETUP_GUIDE.md (guía completa)
✓ UPGRADE_UI_QUICK_REFERENCE.md (esta guía)
```

---

## 🎓 Mejores Prácticas Aplicadas

- ✅ **Separación de responsabilidades:** UI separado de lógica
- ✅ **Componentes modulares:** Cards reutilizables
- ✅ **Inyección de dependencias:** VContainer integration
- ✅ **Interfaces:** IUpgradeUI para testing y flexibilidad
- ✅ **Events:** System.Action para comunicación entre componentes
- ✅ **Animaciones suaves:** Coroutines para fade y scale
- ✅ **Memory management:** Unsubscribe de eventos en OnDestroy

---

**¡Disfruta tu nuevo Upgrade UI System! 🎮**
