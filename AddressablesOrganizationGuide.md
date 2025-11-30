# Estrategia de Organización de Addressables para el Proyecto "Santa"

## 1. Filosofía de Organización
Agruparemos los assets por **Ciclo de Vida de Carga** (¿Cuándo se necesita?) y **Tipo de Contenido**, en lugar de por su ubicación en carpetas.

### Reglas de Oro:
- **No mezclar etiquetas**: Un asset debe pertenecer a un solo grupo lógico de carga.
- **Granularidad Media**: Evitar un grupo por cada asset o un único grupo gigante. Buscamos un balance entre overhead de red y uso de memoria.
- **Separar Datos de Arte**: Los ScriptableObjects (ligeros) se empaquetan aparte de assets pesados como texturas, modelos o audio.

## 2. Estructura de Grupos Recomendada
Te recomiendo crear los siguientes grupos en la ventana `Addressables Groups` (`Window > Asset Management > Addressables > Groups`).

---

### A. Grupo: `Core_Data` (Carga Local)
Assets de configuración esenciales, de bajo peso, que se usan constantemente.
- **Configuración**: `Bundle Mode: Pack Together`, `Compression: LZ4`.
- **Contenido Sugerido**:
  - `Assets/Scriptable Objects/Abilities/*.asset`
  - `Assets/Scriptable Objects/Strategies/*.asset`
  - `Assets/Scriptable Objects/Upgrades/*.asset`
  - `Assets/Scriptable Objects/Levels/*.asset`

---

### B. Grupo: `Core_UI` (Carga Local)
Prefabs de UI críticos o muy comunes.
- **Configuración**: `Bundle Mode: Pack Together`.
- **Contenido Sugerido**:
  - `Assets/Prefabs/UI/CombatUI.prefab`
  - `Assets/Prefabs/UI/VirtualGamepad.prefab`
  - `Assets/Prefabs/UI/PauseMenu.prefab`
  - `Assets/Prefabs/UI/UpgradeUI.prefab`

---

### C. Grupo: `Combat_Arenas` (Carga Local/Remota)
Prefabs de los escenarios de combate. Se empaquetan por separado para poder cargar solo el que se va a jugar.
- **Configuración**: `Bundle Mode: Pack Separately`.
- **Contenido Sugerido**:
  - `Assets/Prefabs/Combate/*.prefab` *(Nota: Tus arenas están aquí, no en una subcarpeta `Arenas`)*

---

### D. Grupo: `Enemies_Common` (Carga Local)
Enemigos que aparecen en la mayoría de los niveles.
- **Configuración**: `Bundle Mode: Pack Together`.
- **Contenido Sugerido**:
  - `Assets/Prefabs/Enemies/BasicEnemy.prefab`

---

### E. Grupo: `Enemies_Bosses` (Carga Remota - Futuro)
Jefes que solo aparecen en momentos puntuales. No deben consumir memoria si no se están usando.
- **Configuración**: `Bundle Mode: Pack Separately`.
- **Contenido Sugerido**:
  - `Assets/Prefabs/Enemies/Boss_SantaMalvado.prefab`

---

## 3. Grupos Futuros (Para Assets de Producción)
Cuando el arte final llegue, crea estos grupos para mantener el proyecto optimizado.

- **F. Grupo: `Audio_Music` (Remoto)**: Canciones de fondo. `Pack Separately`.
- **G. Grupo: `Audio_SFX` (Local)**: Efectos de sonido cortos y frecuentes. `Pack Together`.
- **H. Grupo: `VFX_Common` (Local)**: Efectos visuales comunes (explosiones, impactos). `Pack Together`.

---

## 4. Resumen de Keys y Assets
Basado en tu archivo `AddressableKeys.cs` y la estructura de tu proyecto, estos son los assets que deberías marcar como Addressable.

| Categoría | Key (Address)          | Ubicación Sugerida                                  | Estado Actual      |
|-----------|------------------------|-----------------------------------------------------|--------------------|
| UI        | "CombatUI"             | `Assets/Prefabs/UI/CombatUI.prefab`                 | ✅ Carpeta existe  |
| UI        | "VirtualGamepad"       | `Assets/Prefabs/UI/VirtualGamepad.prefab`           | ✅ Carpeta existe  |
| UI        | "UpgradeUI"            | `Assets/Prefabs/UI/UpgradeUI.prefab`                | ✅ Carpeta existe  |
| Ability   | "Direct"               | `Assets/Scriptable Objects/Abilities/DirectAttack.asset` | ✅ Carpeta existe  |
| Ability   | "Area"                 | `Assets/Scriptable Objects/Abilities/AreaAttack.asset` | ✅ Carpeta existe  |
| Targeting | "SingleEnemyTargeting" | `Assets/Scriptable Objects/Strategies/SingleEnemy.asset` | ✅ Carpeta existe  |

**Acción Requerida**: Asegúrate de que cada uno de estos assets esté marcado como "Addressable" y asignado al grupo correcto.

---

## 📖 MANUAL DE OPERACIONES (Paso a Paso)

### 🛠️ Tarea 1: Asignar un Nuevo Asset a Addressables
1.  **Selecciona** el archivo (Prefab, SO, Audio, etc.) en la ventana `Project`.
2.  Ve al **Inspector**.
3.  En la parte superior, marca la casilla **☑ Addressable**.
4.  **IMPORTANTE**: Unity usará la ruta completa como `Address`. Bórrala y escribe la **key corta y descriptiva** que usarán los programadores.
    - ❌ **Mal**: `Assets/Prefabs/UI/MyNewPanel.prefab`
    - ✅ **Bien**: `MyNewPanel` (Este nombre debe coincidir con `AddressableKeys.cs`).

### 📂 Tarea 2: Mover al Grupo Correcto
Por defecto, los nuevos assets van a `Default Local Group`. Para organizarlos:
1.  Abre la ventana de grupos: `Window > Asset Management > Addressables > Groups`.
2.  Localiza tu nuevo asset en `Default Local Group`.
3.  **Arrastra** el asset al grupo correcto según la estructura definida arriba (ej. una Habilidad va a `Core_Data`).
    - *Tip*: Si un grupo no existe, haz clic derecho -> `Create New Group` -> `Packed Assets` para crearlo.

### 🏗️ Tarea 3: Generar el Contenido (Build)
**¡CRÍTICO!** Sin este paso, los cambios no se aplicarán en el juego.

**¿Cuándo hacerlo?**
- Después de marcar nuevos assets como Addressable.
- Después de mover assets entre grupos.
- Después de cambiar los `Addresses` (keys).
- **Siempre antes de hacer una Build final del juego.**

**Pasos:**
1.  Abre `Window > Asset Management > Addressables > Groups`.
2.  En el menú de esa ventana, ve a `Build > New Build > Default Build Script`.
3.  Espera a que la barra de progreso termine.

### 🧹 Tarea 4: Limpiar Caché (Solución de Problemas)
Si el juego carga assets incorrectos o ves errores de catálogo no encontrado:
1.  Abre `Window > Asset Management > Addressables > Groups`.
2.  Ve a `Build > Clean Build > All`.
3.  Espera a que termine.
4.  **Repite la Tarea 3** (`New Build`) para regenerar todo desde cero.

---

## 🚨 Errores Comunes y Soluciones

| Síntoma                                  | Causa Probable                                          | Solución                                                                    |
|------------------------------------------|---------------------------------------------------------|-----------------------------------------------------------------------------|
| `"Exception: Address [X] not found"`     | El nombre (Address) en el Inspector no coincide con el código. | Revisa mayúsculas/minúsculas y espacios en el `Address` del asset.          |
| Un cambio en un Prefab no se ve en el juego. | No se generó un nuevo build de Addressables.            | Ejecuta la **Tarea 3 (New Build)**.                                         |
| El tamaño de la Build es demasiado grande. | Assets pesados (texturas, audio) en grupos locales. | Mueve assets grandes a grupos remotos o empaquétalos por separado (`Pack Separately`). |
