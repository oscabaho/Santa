# 🎮 GUÍA COMPLETA: Setup Upgrade UI Prefab

## 📑 Índice
1. [Preparación](#1-preparación)
2. [Crear la Jerarquía UI](#2-crear-la-jerarquía-ui)
3. [Configurar los Scripts](#3-configurar-los-scripts)
4. [Crear el Prefab](#4-crear-el-prefab)
5. [Integración con el Sistema](#5-integración-con-el-sistema)
6. [Testing](#6-testing)
7. [Personalización Visual](#7-personalización-visual)

---

## 1. Preparación

### 1.1 Verificar que tienes los scripts creados:
- ✅ `UpgradeUI.cs` (refactorizado)
- ✅ `UpgradeCardUI.cs` (nuevo componente modular)

### 1.2 Estructura de carpetas recomendada:
```
Assets/
  ├── Prefabs/
  │   └── UI/
  │       └── UpgradeUI.prefab (crearemos esto)
  └── Scripts/
      └── Upgrades/
          ├── UpgradeUI.cs
          ├── UpgradeManager.cs
          └── Components/
              └── UpgradeCardUI.cs
```

---

## 2. Crear la Jerarquía UI

### 2.1 Canvas Principal
1. **Hierarchy** → Click derecho → `UI` → `Canvas`
2. Renombrar a: **`UpgradeCanvas`**
3. **Inspector - Canvas Component:**
   - Render Mode: `Screen Space - Overlay`
   - Pixel Perfect: ✅ (opcional)
4. **Inspector - Canvas Scaler:**
   - UI Scale Mode: `Scale With Screen Size`
   - Reference Resolution: `1920 x 1080`
   - Screen Match Mode: `Match Width Or Height`
   - Match: `0.5`

### 2.2 Event System
- Si no existe, Unity lo crea automáticamente
- Verificar que existe **EventSystem** en la Hierarchy

### 2.3 Fondo Oscuro (Dark Background)
1. Click derecho en `UpgradeCanvas` → `UI` → `Image`
2. Renombrar: **`DarkBackground`**
3. **Configurar Rect Transform:**
   - Hacer clic en el cuadrado de Anchor Presets
   - Mantener **ALT + SHIFT** y seleccionar **stretch-stretch** (abajo-derecha)
   - Establecer: Left `0`, Right `0`, Top `0`, Bottom `0`
4. **Configurar Image:**
   - Source Image: (dejar None/default)
   - Color: Negro con transparencia
     - R: `0`, G: `0`, B: `0`, A: `180`

### 2.4 Panel Principal (UpgradePanel)
1. Click derecho en `UpgradeCanvas` → `UI` → `Image`
2. Renombrar: **`UpgradePanel`**
3. **Configurar Rect Transform:**
   - Anchor Preset: `Center-Middle`
   - Width: `1400`
   - Height: `700`
   - Pos X: `0`, Pos Y: `0`, Pos Z: `0`
4. **Configurar Image:**
   - Color: `#2C2C2CFF` (gris oscuro)
5. **Añadir componente Shadow (opcional):**
   - Add Component → `Shadow`
   - Effect Distance: X: `5`, Y: `-5`
   - Effect Color: Negro semitransparente

6. **IMPORTANTE - Añadir CanvasGroup:**
   - Add Component → `Canvas Group`
   - Alpha: `1`
   - Interactable: ✅
   - Block Raycasts: ✅
   - (Esto permite el fade in/out)

### 2.5 Título
1. Click derecho en `UpgradePanel` → `UI` → `Text - TextMeshPro`
   - Si es la primera vez, Unity te pedirá importar TMP Essentials → **Import**
2. Renombrar: **`TitleText`**
3. **Configurar Rect Transform:**
   - Anchor: `Top-Center`
   - Pos Y: `-50`
   - Width: `1200`
   - Height: `100`
4. **Configurar TextMeshProUGUI:**
   - Text: `"¡ELIGE TU MEJORA!"`
   - Font Size: `60`
   - Alignment: Center ⬜ Top
   - Color: Blanco `#FFFFFFFF` o dorado `#FFD700FF`
   - Font Style: **Bold**

---

## 3. Crear las Tarjetas de Upgrade

### 3.1 Tarjeta Opción 1 (Option1Card)

#### A) Crear el contenedor de la tarjeta
1. Click derecho en `UpgradePanel` → `UI` → `Image`
2. Renombrar: **`Option1Card`**
3. **Configurar Rect Transform:**
   - Anchor: `Center-Middle`
   - Width: `550`
   - Height: `500`
   - Pos X: `-380` (izquierda)
   - Pos Y: `-50`
4. **Configurar Image:**
   - Color: `#3C3C3CFF` (gris medio)

#### B) Nombre del Upgrade
1. Click derecho en `Option1Card` → `UI` → `Text - TextMeshPro`
2. Renombrar: **`UpgradeNameText`**
3. **Configurar Rect Transform:**
   - Anchor: `Top-Stretch` (fila superior, centro)
   - Left: `0`, Right: `0`
   - Top: `0`
   - Height: `80`
4. **Configurar TextMeshProUGUI:**
   - Text: `"Nombre del Upgrade"` (placeholder)
   - Font Size: `36`
   - Alignment: Center ⬜ Middle
   - Color: `#FFD700FF` (dorado)
   - Font Style: **Bold**

#### C) Descripción del Upgrade
1. Click derecho en `Option1Card` → `UI` → `Text - TextMeshPro`
2. Renombrar: **`UpgradeDescriptionText`**
3. **Configurar Rect Transform:**
   - Anchor: `Stretch-Stretch`
   - Left: `30`, Right: `30`
   - Top: `100`, Bottom: `120`
4. **Configurar TextMeshProUGUI:**
   - Text: `"Descripción detallada del upgrade..."` (placeholder)
   - Font Size: `24`
   - Alignment: Top ⬜ Left
   - Color: `#E0E0E0FF` (gris claro)
   - Wrapping: ✅ **Enabled**
   - Overflow: `Ellipsis` (opcional)

#### D) Botón de Selección
1. Click derecho en `Option1Card` → `UI` → `Button - TextMeshPro`
2. Renombrar: **`SelectButton`**
3. **Configurar Rect Transform del Button:**
   - Anchor: `Bottom-Stretch`
   - Left: `40`, Right: `40`, Bottom: `20`
   - Height: `70`
4. **Configurar Button Component:**
   - Normal Color: `#4C4C4CFF`
   - Highlighted Color: `#6C6C6CFF`
   - Pressed Color: `#2C2C2CFF`
   - Selected Color: `#5C5C5CFF`
5. **Configurar el texto del botón:**
   - Seleccionar el hijo **Text (TMP)** del Button
   - Text: `"SELECCIONAR"`
   - Font Size: `32`
   - Alignment: Center ⬜ Middle
   - Color: Blanco `#FFFFFFFF`
   - Font Style: **Bold**

#### E) (Opcional) Icono del Upgrade
1. Click derecho en `Option1Card` → `UI` → `Image`
2. Renombrar: **`UpgradeIcon`**
3. **Configurar Rect Transform:**
   - Anchor: `Top-Center`
   - Width: `120`, Height: `120`
   - Pos Y: `-50` (debajo del nombre)
4. **Configurar Image:**
   - Color: Blanco (o color acento)
   - Preserve Aspect: ✅

### 3.2 Duplicar para Opción 2
1. Seleccionar **`Option1Card`**
2. Duplicar: `Ctrl + D` (Windows) o `Cmd + D` (Mac)
3. Renombrar el duplicado: **`Option2Card`**
4. **Cambiar Pos X a:** `+380` (derecha)

---

## 4. Configurar los Scripts

### 4.1 Configurar UpgradeCardUI en Option1Card

1. Seleccionar **`Option1Card`** en Hierarchy
2. **Add Component** → buscar `UpgradeCardUI`
3. **Arrastrar las referencias:**
   - **Upgrade Name Text** → arrastra `UpgradeNameText`
   - **Upgrade Description Text** → arrastra `UpgradeDescriptionText`
   - **Select Button** → arrastra `SelectButton`
   - **Upgrade Icon** → arrastra `UpgradeIcon` (si lo creaste)
4. **Visual Settings (opcional):**
   - Normal Color: `#3C3C3CFF`
   - Hover Color: `#4C4C4CFF`

### 4.2 Configurar UpgradeCardUI en Option2Card

Repetir los mismos pasos que 4.1 para **`Option2Card`**.

### 4.3 Configurar UpgradeUI en UpgradePanel

1. Seleccionar **`UpgradePanel`** en Hierarchy
2. **Add Component** → buscar `UpgradeUI`
3. **Arrastrar las referencias:**
   - **Upgrade Panel** → arrastra `UpgradePanel` (sí mismo)
   - **Canvas Group** → arrastra el CanvasGroup del `UpgradePanel`
   - **Option1 Card** → arrastra `Option1Card`
   - **Option2 Card** → arrastra `Option2Card`
   - **Title Text** → arrastra `TitleText` (opcional)
   - **Close Button** → deja vacío por ahora (opcional)
4. **Animation Settings:**
   - Fade In Duration: `0.3` (o al gusto)

---

## 5. Crear el Prefab

### 5.1 Convertir en Prefab
1. Seleccionar **`UpgradeCanvas`** en la Hierarchy
2. Arrastrarlo a la carpeta: `Assets/Prefabs/UI/`
3. Unity lo convertirá en un prefab azul
4. **Renombrar el prefab:** `UpgradeUI.prefab`

### 5.2 Verificar el Prefab
1. Hacer doble-clic en el prefab para abrirlo en Prefab Mode
2. Verificar que todas las referencias estén conectadas
3. Salir del Prefab Mode

### 5.3 Limpiar la Scene
- Ahora puedes **eliminar** el `UpgradeCanvas` de la Hierarchy
- El prefab está guardado en la carpeta

---

## 6. Integración con el Sistema

### 6.1 Configurar VContainer para Instancia Dinámica

Ya que tu UI varía según el estado del juego, necesitas instanciarla dinámicamente con VContainer.

#### Paso 1: Crear o modificar tu LifetimeScope

1. Localiza tu `LifetimeScope` principal (usualmente en tu escena de juego o en un GameObject persistente)
2. Si no tienes uno, crea un nuevo GameObject y añade el componente `LifetimeScope`

#### Paso 2: Configurar el registro del prefab

En tu script de configuración de VContainer (LifetimeScope o un Installer), añade:

```csharp
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GameLifetimeScope : LifetimeScope
{
    [Header("UI Prefabs")]
    [SerializeField] private UpgradeUI upgradeUIPrefab;

    protected override void Configure(IContainerBuilder builder)
    {
        // Registrar el prefab de UpgradeUI
        // Se instanciará automáticamente como Singleton
        builder.RegisterComponentInNewPrefab(upgradeUIPrefab, Lifetime.Singleton)
            .AsImplementedInterfaces()  // Registra como IUpgradeUI
            .AsSelf();                   // También como UpgradeUI
        
        // Tus otros registros...
        // builder.Register<UpgradeManager>(Lifetime.Singleton).AsImplementedInterfaces();
        // etc.
    }
}
```

#### Paso 3: Asignar el prefab en el Inspector

1. Selecciona tu GameObject con el `LifetimeScope` en la Hierarchy
2. En el Inspector, arrastra el prefab `UpgradeUI.prefab` al campo **Upgrade UI Prefab**

**IMPORTANTE:** No coloques el prefab directamente en la escena. VContainer lo instanciará automáticamente cuando sea necesario.

#### Paso 4: (Opcional) Controlar el Canvas Parent

Si quieres que la UI se instancie bajo un Canvas específico:

```csharp
[Header("UI Settings")]
[SerializeField] private UpgradeUI upgradeUIPrefab;
[SerializeField] private Transform uiRoot; // Canvas donde se instanciará

protected override void Configure(IContainerBuilder builder)
{
    builder.RegisterComponentInNewPrefab(upgradeUIPrefab, Lifetime.Singleton)
        .UnderTransform(uiRoot) // Se instanciará como hijo de uiRoot
        .AsImplementedInterfaces()
        .AsSelf();
}
```

### 6.2 Verificar la Inyección

Tu `UpgradeManager` ya está configurado para recibir `IUpgradeUI`:

```csharp
[Inject]
public void Construct(IUpgradeUI upgradeUI, ICombatTransitionService combatTransitionService)
{
    _upgradeUI = upgradeUI;
    _combatTransitionService = combatTransitionService;
}
```

---

## 7. Testing

### 7.1 Test Básico en Editor

1. Crea un script de testing temporal:

```csharp
using UnityEngine;
using VContainer;

public class UpgradeUITester : MonoBehaviour
{
    [SerializeField] private AbilityUpgrade testUpgrade1;
    [SerializeField] private AbilityUpgrade testUpgrade2;

    private IUpgradeUI _upgradeUI;

    [Inject]
    public void Construct(IUpgradeUI upgradeUI)
    {
        _upgradeUI = upgradeUI;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestShowUpgrades();
        }
    }

    private void TestShowUpgrades()
    {
        if (testUpgrade1 != null && testUpgrade2 != null)
        {
            _upgradeUI?.ShowUpgrades(testUpgrade1, testUpgrade2);
        }
        else
        {
            Debug.LogWarning("Assign test upgrades in Inspector!");
        }
    }
}
```

2. Crea 2 **AbilityUpgrade ScriptableObjects** para testing
3. Presiona **T** en Play Mode para mostrar la UI

### 7.2 Verificar Funcionalidad

✅ **Checklist:**
- [ ] El panel aparece con fade-in
- [ ] Las tarjetas muestran el nombre y descripción correctos
- [ ] Los botones son clickeables
- [ ] Al hacer clic, se aplica el upgrade y se oculta el panel
- [ ] El flujo de combate continúa correctamente

---

## 8. Personalización Visual

### 8.1 Añadir Animaciones (opcional)

Puedes usar **Animator** o **DOTween** para:
- Entrada de tarjetas con escalado
- Hover effects en los botones
- Partículas al seleccionar

### 8.2 Añadir Sonidos

En `UpgradeUI.cs`, añade:

```csharp
[Header("Audio")]
[SerializeField] private AudioClip showSound;
[SerializeField] private AudioClip selectSound;
[SerializeField] private AudioSource audioSource;

private void Show()
{
    // ... código existente ...
    audioSource?.PlayOneShot(showSound);
}

private void OnUpgradeChosen(AbilityUpgrade chosenUpgrade)
{
    audioSource?.PlayOneShot(selectSound);
    // ... resto del código ...
}
```

### 8.3 Responsive Design

Tu Canvas Scaler ya está configurado, pero puedes ajustar:
- **Match** para priorizar ancho/alto según tu juego
- Tamaños de fuente relativos con **Auto Size**
- Layout Groups para organización automática

---

## 9. Mejoras Futuras (Opcionales)

### 9.1 Sistema de Rareza de Upgrades
Añade colores según rareza (común, raro, épico, legendario).

### 9.2 Preview de Stats
Muestra "antes → después" de los stats afectados.

### 9.3 Sistema de Reroll
Botón para gastar moneda y obtener nuevas opciones.

### 9.4 Historial de Upgrades
Panel que muestra todos los upgrades obtenidos en la run.

---

## 🎉 ¡Listo!

Tu Upgrade UI ahora es:
- ✅ **Modular** (componentes reutilizables)
- ✅ **Escalable** (fácil añadir más opciones)
- ✅ **Profesional** (animaciones y polish)
- ✅ **Mantenible** (código limpio y documentado)

---

## 📞 Solución de Problemas Comunes

### Problema: "Las referencias son None/null"
**Solución:** Asegúrate de arrastrar los GameObjects correctos en el Inspector del prefab.

### Problema: "El panel no se muestra"
**Solución:** Verifica que el Canvas esté en Screen Space - Overlay y que el CanvasGroup tenga Alpha = 1.

### Problema: "Los botones no responden"
**Solución:** Asegúrate de que existe un EventSystem en la escena.

### Problema: "El texto no se ve"
**Solución:** Verifica que TextMeshPro esté importado (Window → TextMeshPro → Import TMP Essential Resources).

---

**Fecha de creación:** Noviembre 2025  
**Versión:** 1.0  
**Proyecto:** Santa - Upgrade System
