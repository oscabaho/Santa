# Guía paso a paso: Configurar Exploración en TestScene

Esta guía detalla cómo montar la sección de exploración en tu **TestScene** basándose en tu jerarquía actual (screenshot) y los sistemas ya implementados.

---

## 📋 Estado actual de tu TestScene (según screenshot)

```
TestScene
├── Main Camera
├── Directional Light
├── GameLifetimeScope
├── UIManager
├── TurnBasedCombatManager
├── CombatTransitionManager
├── UpgradeManager
├── GameStateManager
├── GameplayUIManager
├── LevelManager
├── ScreenFade
├── CombatCameras
│   ├── MainCombatCamera
│   ├── TargetSelectionCamera
│   └── CombatCameraManager
├── Player
│   ├── PlayerInteraction
│   └── CombatTestInitiator
├── EventSystem
└── UpgradeUI
    └── CombatUI
```

---

## 🎯 Objetivo

Añadir exploración completa a TestScene para que:
1. El jugador pueda moverse libremente por la escena
2. Al acercarse a un trigger de combate, aparezca un botón de acción
3. Al presionar Interact, inicie el combate con transición de cámara
4. Las cámaras de combate solo se activen durante combate

---

## 📦 Paso 1: Configurar el Player para Exploración

### 1.1 Reorganizar componentes existentes (IMPORTANTE)

**⚠️ SI `PlayerInteraction` y `CombatTestInitiator` están como GameObjects HIJOS** en tu jerarquía actual (como muestra tu screenshot), necesitas reorganizarlos:

**Pasos para reorganizar:**

1. Selecciona el GameObject hijo **PlayerInteraction** (bajo Player)
2. En el Inspector, identifica si tiene algún componente script llamado `PlayerInteraction`
3. Si tiene el componente:
   - Click derecho en el componente → **Copy Component**
   - Selecciona el GameObject padre **Player**
   - Click derecho en el Inspector → **Paste Component As New**
   - Verifica que el `InputReader` se haya copiado correctamente (si estaba asignado)
4. Elimina el GameObject hijo **PlayerInteraction** de la jerarquía (selecciónalo y presiona Delete)

5. Repite el mismo proceso con **CombatTestInitiator**:
   - Selecciona el GameObject hijo **CombatTestInitiator**
   - Copy Component → Paste en el GameObject padre **Player**
   - Elimina el GameObject hijo **CombatTestInitiator**

**Alternativa más rápida (si los hijos NO tienen otros componentes importantes):**
- Simplemente elimina los GameObjects hijos `PlayerInteraction` y `CombatTestInitiator`
- Luego agrega los componentes frescos al GameObject `Player` siguiendo el paso 1.2

### 1.2 Agregar componentes al Player existente

En tu GameObject **Player** (ya existe en la jerarquía):

**⚠️ IMPORTANTE**: Todos estos son **componentes (MonoBehaviour)** que se agregan **directamente al GameObject `Player`**, NO son GameObjects hijos. Se añaden usando el botón "Add Component" en el Inspector.

**Componentes necesarios:**
- 🔄 `PlayerInteraction` (reorganizar desde hijo a componente directo)
- 🔄 `CombatTestInitiator` (reorganizar desde hijo a componente directo)
- 🆕 `CharacterController` (componente nuevo)
- 🆕 `ActionPointComponentBehaviour` (componente nuevo)
- 🆕 `HealthComponentBehaviour` (componente nuevo)
- 🆕 `PlayerComponentRegistry` (componente nuevo)
- 🆕 `ExplorationPlayerIdentifier` (componente nuevo)
- 🆕 `Movement` (componente nuevo)

**Cómo agregarlos:**

1. Selecciona el GameObject **Player** en la jerarquía
2. En el Inspector (panel derecho), baja hasta el final de los componentes existentes
3. Haz clic en el botón **"Add Component"**
4. En el campo de búsqueda que aparece, escribe el nombre del componente (ej: "CharacterController")
5. Selecciona el componente de la lista
6. Repite los pasos 3-5 para cada componente marcado con 🆕 (y 🔄 si los eliminaste en el paso 1.1)

**Resultado esperado:** 
- Tu GameObject `Player` NO debe tener GameObjects hijos llamados `PlayerInteraction` o `CombatTestInitiator`
- Tu GameObject `Player` debe tener **8 componentes** en total, todos visibles en el Inspector como una lista vertical de componentes

### 1.2 Configurar CharacterController

Una vez agregado el `CharacterController`:
- **Center**: `(0, 1, 0)` (ajusta según la altura de tu modelo)
- **Radius**: `0.5`
- **Height**: `2` (ajusta según tu modelo)
- **Slope Limit**: `45`
- **Step Offset**: `0.3`

### 1.3 Configurar ActionPointComponentBehaviour

- **Max Value**: `3` (o el valor que uses para AP iniciales)
- **Initial Value**: `3`

### 1.4 Configurar HealthComponentBehaviour

- **Max Value**: `100` (o el valor que uses para vida inicial)
- **Initial Value**: `100`

### 1.5 Configurar Movement

1. En el componente `Movement`, busca el campo **Input Reader**
2. Arrastra tu ScriptableObject `InputReader` desde `Assets/` (búscalo en tu carpeta de ScriptableObjects o Resources)
3. **Move Speed**: `5` (ajusta a tu gusto)
4. **Rotation Speed**: `720`
5. **Gravity Value**: `-9.81`

### 1.6 Configurar PlayerInteraction

1. En el componente `PlayerInteraction`, busca el campo **Input Reader**
2. Arrastra el mismo ScriptableObject `InputReader` que usaste en Movement

### 1.7 Configurar CombatTestInitiator

1. En el componente `CombatTestInitiator`:
2. **❌ Desactiva** el checkbox **Auto Start On Play**
3. Dejar `Player Tag` = `"Player"`
4. Dejar `Enemy Tag` = `"Enemy"`

### 1.8 Asignar tag al Player

1. Con el GameObject **Player** seleccionado
2. En el Inspector, arriba, hay un dropdown que dice "Untagged"
3. Selecciona **"Player"** (si no existe, créalo: Add Tag... → + → "Player")

---

## 🎮 Paso 2: Crear el InputReader (si no existe)

Si aún no tienes el ScriptableObject `InputReader`:

1. En la carpeta `Assets/Scripts/` o `Assets/Scriptable Objects/`
2. Click derecho → Create → Santa → **Input Reader**
3. Nómbralo `InputReader`
4. Este ScriptableObject gestiona los eventos de input (Move, Interact)

---

## 📱 Paso 3: Configurar UI de Exploración

### 3.1 Crear GameObject para VirtualGamepadUI

**⚠️ IMPORTANTE**: `VirtualGamepadUI` es un **GameObject con un componente**, no solo un componente. Debe tener un hijo llamado "ActionButton".

1. En la jerarquía, click derecho en el espacio vacío → **Create Empty**
2. Nómbralo **"VirtualGamepadUI"** (este es el GameObject padre)
3. Con el GameObject seleccionado, click en **"Add Component"** en el Inspector
4. Busca y agrega el componente **`VirtualGamepadUI`**

### 3.2 Crear el Action Button (hijo de VirtualGamepadUI)

**⚠️ IMPORTANTE**: El `ActionButton` es un **GameObject hijo** de `VirtualGamepadUI`, no un componente.

1. En la jerarquía, click derecho sobre el GameObject **VirtualGamepadUI** → UI → **Button - TextMeshPro**
2. Unity te preguntará si quieres crear un Canvas (si no hay uno): click en **"Yes"**
3. Se creará automáticamente una estructura: Canvas → VirtualGamepadUI → ActionButton
4. Renombra el botón hijo a **"ActionButton"** (si Unity no lo nombró así)
5. Este botón se mostrará cuando el jugador esté cerca de un trigger

**Estructura esperada en la jerarquía:**
```
Canvas (creado automáticamente si no existe)
└── VirtualGamepadUI (GameObject con componente VirtualGamepadUI)
    └── ActionButton (GameObject con componente Button)
```

### 3.3 Configurar VirtualGamepadUI

1. Selecciona **VirtualGamepadUI**
2. En el componente `VirtualGamepadUI`, busca el campo **Action Button**
3. Arrastra el GameObject **ActionButton** (hijo) a este campo

### 3.4 Ajustar Canvas (si es necesario)

Si tu **ActionButton** necesita un Canvas:
- VirtualGamepadUI debe estar bajo un Canvas o tener su propio Canvas
- **Canvas Render Mode**: Screen Space - Overlay
- **Canvas Scaler**: Scale With Screen Size (opcional pero recomendado)

---

## 🎯 Paso 4: Crear Trigger de Combate

### 4.1 Crear el GameObject Trigger

**⚠️ IMPORTANTE**: `CombatTrigger_01` es un **GameObject vacío** al que le agregaremos componentes.

1. En la jerarquía, click derecho → **Create Empty**
2. Nómbralo **"CombatTrigger_01"** (o el nombre que prefieras)
3. Posiciónalo en el mundo donde quieras que el jugador encuentre un enemigo

### 4.2 Agregar Collider al Trigger

**⚠️ IMPORTANTE**: El collider es un **componente** que se agrega al GameObject `CombatTrigger_01`.

1. Con **CombatTrigger_01** seleccionado, click en **"Add Component"** en el Inspector
2. Busca y agrega **Box Collider** (o Sphere Collider)
3. **✅ Activa** el checkbox **"Is Trigger"** en el componente Collider
4. Ajusta el tamaño del collider para que sea lo suficientemente grande para detectar al jugador
   - Por ejemplo: `Size = (3, 2, 3)` para un área amplia

### 4.3 Agregar componentes al Trigger

**⚠️ IMPORTANTE**: Estos son **componentes** que se agregan al GameObject `CombatTrigger_01`, NO son GameObjects hijos.

Con **CombatTrigger_01** seleccionado:

1. Click en **"Add Component"** → busca y agrega **`CombatEncounter`**
2. Click en **"Add Component"** → busca y agrega **`CombatTrigger`**

**Resultado esperado:** Tu GameObject `CombatTrigger_01` debe tener **3 componentes**:
- Transform (siempre presente)
- Box Collider (o Sphere Collider) con "Is Trigger" activado
- CombatEncounter
- CombatTrigger

### 4.4 Configurar CombatEncounter

1. En el componente `CombatEncounter`:
2. Configura el **Pool Key** o la referencia al encuentro que quieras usar
3. Si usas pooling, asegúrate de que la key coincida con tu configuración en `CombatScenePool`

---

## 📷 Paso 5: Verificar Configuración de Cámaras

### 5.1 Main Camera

Tu **Main Camera** (ya existente) debe tener:
- ✅ **CinemachineBrain** (componente de Cinemachine)
- **Tag**: `MainCamera`
- **NO desactivar** este GameObject nunca

### 5.2 CombatCameras

Tu GameObject **CombatCameras** (ya existente) contiene:
- **MainCombatCamera** (CinemachineCamera)
- **TargetSelectionCamera** (CinemachineCamera)
- **CombatCameraManager** (componente)

**Verificar CombatCameraManager:**
1. Selecciona **CombatCameras**
2. En el componente `CombatCameraManager`:
   - **Main Combat Camera**: arrastra **MainCombatCamera**
   - **Target Selection Camera**: arrastra **TargetSelectionCamera**

**Estado inicial de las vcams de combate:**
- Al iniciar la escena, ambas deben estar **activas** en la jerarquía (checkbox verde)
- El `CombatCameraManager` las apagará automáticamente en Awake/Construct
- Durante exploración, permanecerán inactivas
- Durante combate, se activarán según la fase

### 5.3 Cámara de Exploración (opcional pero recomendado)

Para tener mejor control de la cámara en exploración, puedes crear una vcam:

**⚠️ IMPORTANTE**: La `ExplorationCamera` es un **GameObject con componente CinemachineCamera**. Se crea automáticamente con el menú de Cinemachine.

1. Click derecho en la jerarquía → Cinemachine → **Cinemachine Camera**
2. Unity crea automáticamente un GameObject con el componente `CinemachineCamera`
3. Renombra el GameObject a **"ExplorationCamera"**
4. Configura el componente CinemachineCamera:
   - **Tracking Target**: arrastra tu GameObject **Player** desde la jerarquía
   - **Look At Target**: arrastra tu GameObject **Player** (o deja vacío para cámara fija relativa)
   - **Lens > Field of View**: `60` (ajusta a tu gusto)
   - **Body**: puede ser "Third Person" o "Orbital Follow" (para cámara que siga al jugador)
   - **Priority**: `10` (mayor que las de combate cuando están inactivas)

**Resultado esperado en la jerarquía:**
```
ExplorationCamera (GameObject con componente CinemachineCamera)
```

Si usas esta vcam de exploración:
- Durante exploración, esta vcam tendrá control del Brain
- Durante combate, las vcams de combate tomarán control automáticamente (prioridad 100)

---

## 🎨 Paso 6: Configurar Managers y LifetimeScope

### 6.1 Verificar GameLifetimeScope

Tu **GameLifetimeScope** (ya existente) debe tener referencias a:
- ✅ UIManager
- ✅ TurnBasedCombatManager
- ✅ CombatTransitionManager
- ✅ UpgradeManager
- ✅ GameStateManager
- ✅ GameplayUIManager
- ✅ LevelManager

**Verificar en el Inspector:**
1. Selecciona **GameLifetimeScope**
2. Asegúrate de que cada campo tenga su correspondiente manager asignado

### 6.2 Verificar CombatTransitionManager

1. Selecciona **CombatTransitionManager**
2. Los campos **Start Combat Sequence** y **End Combat Sequence** pueden estar vacíos por ahora
3. Los crearemos en el siguiente paso (Transiciones)

---

## 🔄 Paso 7: Crear el Terreno/Mundo de Exploración (básico)

### 7.1 Crear un Plane para caminar

1. Click derecho en jerarquía → 3D Object → **Plane**
2. Nómbralo **"Ground"**
3. Escálalo: `Scale = (10, 1, 10)` para tener un área grande
4. Posición: `(0, 0, 0)`

### 7.2 Posicionar el Player

1. Selecciona **Player**
2. Posiciónalo sobre el plano: `Position = (0, 1, 0)`
3. Asegúrate de que el `CharacterController` toque el suelo

### 7.3 Posicionar el CombatTrigger

1. Selecciona **CombatTrigger_01**
2. Posiciónalo lejos del spawn del player: `Position = (5, 1, 5)` (por ejemplo)
3. Así el jugador tiene que caminar para llegar al trigger

---

## ✅ Paso 8: Verificación Final

### 8.1 Checklist de GameObjects

En tu jerarquía debes tener:

```
TestScene
├── Main Camera (con CinemachineBrain) ✅
├── Directional Light ✅
├── Ground (Plane) 🆕
├── GameLifetimeScope ✅
├── UIManager ✅
├── TurnBasedCombatManager ✅
├── CombatTransitionManager ✅
├── UpgradeManager ✅
├── GameStateManager ✅
├── GameplayUIManager ✅
├── LevelManager ✅
├── ScreenFade ✅
├── CombatCameras ✅
│   ├── MainCombatCamera ✅
│   ├── TargetSelectionCamera ✅
│   └── CombatCameraManager ✅
├── ExplorationCamera (opcional) 🆕
├── Player ✅
│   ├── CharacterController 🆕
│   ├── ActionPointComponentBehaviour 🆕
│   ├── HealthComponentBehaviour 🆕
│   ├── PlayerComponentRegistry 🆕
│   ├── ExplorationPlayerIdentifier 🆕
│   ├── Movement 🆕
│   ├── PlayerInteraction ✅
│   └── CombatTestInitiator ✅
├── VirtualGamepadUI 🆕
│   └── ActionButton (Button) 🆕
├── CombatTrigger_01 🆕
│   ├── Box Collider (Is Trigger = ON) 🆕
│   ├── CombatEncounter 🆕
│   └── CombatTrigger 🆕
├── EventSystem ✅
└── UpgradeUI ✅
    └── CombatUI ✅
```

### 8.2 Checklist de Referencias

Verifica que estos campos estén asignados:

**Player:**
- ✅ Movement → Input Reader (ScriptableObject)
- ✅ PlayerInteraction → Input Reader (ScriptableObject)
- ✅ CombatTestInitiator → Auto Start On Play = **OFF**
- ✅ Tag = "Player"

**VirtualGamepadUI:**
- ✅ Action Button (GameObject hijo)

**CombatCameraManager:**
- ✅ Main Combat Camera (CinemachineCamera)
- ✅ Target Selection Camera (CinemachineCamera)

**GameLifetimeScope:**
- ✅ Todos los managers asignados

**CombatTrigger_01:**
- ✅ Collider con "Is Trigger" activado
- ✅ CombatEncounter configurado con Pool Key o referencias

---

## 🎮 Paso 9: Prueba Básica

### 9.1 Primera ejecución

1. Guarda la escena (Ctrl+S)
2. Dale Play
3. **Esperado:**
   - El jugador está en el mundo
   - No hay combate automático
   - Las cámaras de combate están inactivas
   - La Main Camera (o ExplorationCamera si la creaste) muestra la escena

### 9.2 Probar movimiento

1. Con Play activo:
2. Usa las teclas **WASD** o el stick del gamepad
3. **Esperado:**
   - El jugador se mueve por el plano
   - La cámara lo sigue (si usas ExplorationCamera)
   - El CharacterController previene atravesar el suelo

### 9.3 Probar trigger de combate

1. Mueve el jugador hacia el **CombatTrigger_01**
2. **Esperado:**
   - Al entrar en el collider, aparece el **Action Button** en pantalla
3. Presiona la tecla de Interact (por defecto **E** o el botón configurado en InputReader)
4. **Esperado:**
   - Se inicia el combate
   - Las cámaras de combate se activan
   - La UI de combate aparece

### 9.4 Si algo no funciona

**Problema: El player no se mueve**
- Verifica que `Movement` tenga el `InputReader` asignado
- Verifica que el `InputReader` tenga el Action Map configurado
- Verifica que el `CharacterController` esté presente

**Problema: No aparece el Action Button**
- Verifica que `VirtualGamepadUI` tenga el botón hijo asignado
- Verifica que el `CombatTrigger_01` tenga "Is Trigger" activado
- Verifica que `PlayerInteraction` tenga el `InputReader` asignado
- Revisa los logs en Console para ver si hay errores de inyección

**Problema: El combate no inicia**
- Verifica que `CombatTestInitiator` tenga "Auto Start On Play" **desactivado**
- Verifica que `CombatEncounter` esté configurado correctamente
- Verifica que el `Player` tenga el tag "Player"
- Revisa la Console para errores de `CombatTrigger` o `CombatScenePool`

**Problema: Las cámaras de combate están activas en exploración**
- Verifica que `CombatCameraManager` esté inyectado correctamente
- Revisa que `GameStateManager` esté en la escena y registrado en `GameLifetimeScope`
- Los logs deben mostrar "Combat cameras set to INACTIVE (Exploration mode)." al inicio

---

## 🎬 Siguiente Paso: Transiciones

Una vez que tengas la exploración funcionando básicamente, el siguiente paso es configurar las **TransitionSequences** para que las transiciones entre exploración y combate sean visuales y fluidas:

- Fade in/out de pantalla
- Activar/desactivar UI según el estado
- (Opcional) Activar/desactivar componentes de movimiento durante combate

Estas transiciones se configuran en el `CombatTransitionManager` y usan `TransitionTask` (ScriptableObjects).

---

## 📝 Notas Finales

- **No desactivar la Main Camera**: Tiene el CinemachineBrain que Cinemachine necesita para renderizar
- **Las vcams de combate se gestionan automáticamente**: `CombatCameraManager` las activa/desactiva según el GameState
- **InputReader es ScriptableObject**: Se comparte entre componentes; editarlo afecta a todos
- **Tags importantes**: "Player" para detectar al jugador en scripts
- **VContainer inyección**: Los managers se inyectan vía `GameLifetimeScope`; si algo falla, revisa que estén registrados ahí

---

¡Con esto deberías tener la exploración completamente funcional en tu TestScene! 🎉

Cuando estés listo, puedo generarte las **TransitionSequences** para las transiciones visuales entre exploración y combate.
