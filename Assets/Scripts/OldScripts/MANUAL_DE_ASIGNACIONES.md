
# Manual de Implementación y Asignación de Scripts (Unity)

> **¡Bienvenido!** Este manual está pensado para personas con poca o ninguna experiencia previa en Unity. Aquí aprenderás, paso a paso, cómo preparar tus escenas, qué scripts debes asignar a cada GameObject y cómo organizar tu proyecto para que todo funcione correctamente.

---

---

## Índice

1. Introducción rápida al proyecto
2. Estructura recomendada de carpetas y archivos
3. Escenas principales y su propósito
4. Entidades y prefabs clave
5. Asignación manual de scripts por entidad y escena
6. Ubicación de scripts en la jerarquía de Unity
7. Configuración de componentes y variables en el Inspector
8. Flujo de datos persistentes entre escenas
9. Consejos y buenas prácticas para principiantes
10. Preguntas frecuentes y solución de problemas

---

## 1. Introducción rápida al proyecto
Este proyecto es un Survival Horror con elementos RPG y narrativa, donde el jugador explora, combate y resuelve misterios en una aldea maldita. El ciclo día/noche y la gestión de vida, stamina, inventario y eventos son fundamentales.


## 2. Estructura recomendada de carpetas y archivos

- `Assets/Scripts/MonoBehaviours/` — Scripts que se asignan a GameObjects en la escena (por ejemplo, control de jugador, enemigos, UI).
- `Assets/Scripts/Components/` — Scripts que definen la lógica interna de estadísticas o sistemas (por ejemplo, vida, stamina).
- `Assets/Scripts/Stats/` — Scripts para estadísticas base reutilizables.
- `Assets/Prefabs/` — Prefabricados: plantillas de objetos que puedes reutilizar (jugador, enemigos, armas, etc.).
- `Assets/Scenes/` — Tus escenas principales (Exploración, Combate, Menú, etc.).

> **¿Qué es un prefab?**
> Un prefab es como un "molde" de un objeto. Puedes crear copias idénticas en la escena y, si cambias el prefab, todos los objetos basados en él se actualizan automáticamente.


## 3. Escenas principales y su propósito

- **Exploración**: El jugador recorre el mundo, habla con NPCs y busca pistas. Aquí no hay combate.
- **Combate**: El jugador pelea contra enemigos. Aquí se usan los sistemas de vida, stamina y ataque.
- **Menú/Inventario**: El jugador gestiona su equipo, consulta logros y misiones.

> **¿Qué es una escena?**
> Una escena en Unity es como un "nivel" o "pantalla". Puedes tener varias escenas y cambiar entre ellas (por ejemplo, pasar de exploración a combate).


## 4. Entidades y prefabs clave

- **Jugador**: El personaje que controlas. Debe tener todos los scripts de movimiento, vida, stamina, inventario y ataque.
- **Enemigos**: Personajes controlados por la IA. Tienen scripts de comportamiento y salud.
- **Objetos destruibles**: Cosas que el jugador puede romper (cajas, barriles, etc.).
- **Armas**: Objetos que el jugador puede equipar. Se gestionan desde ScriptableObjects (archivos de datos, no scripts en la escena).
- **UI**: Elementos de la interfaz (inventario, barra de vida, logros, misiones).

> **¿Qué es un GameObject?**
> Es cualquier objeto en tu escena de Unity: un personaje, una luz, una cámara, un botón, etc.

---
# Manual de Asignaciones de Scripts (Unity)

Este documento sirve como guía rápida para la asignación manual de scripts y componentes clave en tu proyecto. Aquí encontrarás qué scripts debes añadir manualmente a cada GameObject relevante y recomendaciones para la organización de la escena.

---


## 5. Asignación manual de scripts por entidad y escena


### Player (Jugador)

**¿Cómo armar el jugador?**
1. Crea un GameObject vacío llamado `Player`.
2. Añade los siguientes scripts al GameObject `Player`:
   - `PaperMarioPlayerMovement` (movimiento)
   - `PlayerInventory` (inventario)
   - `PlayerEquipmentController` (equipamiento)
   - `StaminaComponentBehaviour` (stamina)
   - `HealthComponentBehaviour` (vida)
   - `WeaponMasteryComponent` (maestría de armas)
   - `AttackComponent` (ataque)
  
      Para cada script, asigna y verifica los campos públicos en el Inspector antes de ejecutar la escena.

      Campos y checks recomendados en el Inspector (Player):
      - `PaperMarioPlayerMovement`:
         - `InputActionAsset inputActions` (arrastrar el asset de Input)
         - `float moveSpeed`, `float sprintMultiplier`
      - `PlayerInventory`:
         - `int maxSlots` (por defecto 5)
         - `List<InventoryItem> initialItems` (arrastrar ítems iniciales)
      - `PlayerEquipmentController`:
         - `Transform weaponMount` (donde se instancian los hitboxes)
         - `WeaponInstance EquippedWeaponInstance` (se muestra en tiempo de ejecución)
      - `StaminaComponentBehaviour` / `HealthComponentBehaviour`:
         - `float maxStamina` / `float maxHealth`
         - `float regenRate`
      - `AttackComponent`:
         - `WeaponItem defaultWeapon` (opcional)
         - `int staminaCost` (por ataque)

      Checklist antes de play:
      - Asegúrate de que `Player` tenga `HealthComponentBehaviour` y `StaminaComponentBehaviour` en el mismo GameObject o en un componente accesible.
      - El `InputActionAsset` debe estar configurado y referenciado.
      - Los prefabs de hitbox referenciados en `WeaponItem` deben tener `WeaponHitbox` y colisionadores configurados.
3. Si tienes un modelo 3D o sprite, agrégalo como hijo de `Player` y ponle el script `BillboardCharacter`.
4. Si usas cámara propia, crea un GameObject hijo de `Player` llamado `CameraPivot` y asígnale el script `PaperMarioCameraController`.

> **Nota:** Para que el movimiento funcione, debes tener un `InputActionAsset` creado y asignado en el campo correspondiente del script `PaperMarioPlayerMovement`.

**Ejemplo de jerarquía:**
```
Player
├── ModeloVisual (con BillboardCharacter)
├── CameraPivot (con PaperMarioCameraController)
```


### Enemigos

1. Crea un GameObject vacío llamado `Enemy`.
2. Añade los scripts:
   - `Enemy` (comportamiento)
   - `EnemyHealthController` (salud y muerte)
   - `HealthComponentBehaviour` (vida)
   - (Opcional) `KryptoniteDebuff` si el enemigo puede ser debilitado por un ítem.

      Campos y checks recomendados en el Inspector (Enemy):
      - `Enemy`:
         - `float detectionRadius`, `LayerMask targetLayers`
         - `AIState initialState`
      - `EnemyHealthController`:
         - `float maxHealth`
         - `bool destroyOnDeath` (si el objeto se elimina)

      Checklist antes de play:
      - El prefab `Enemy` debe tener colliders y Rigidbody según su tipo (kinematic o dinámico).
      - Los puntos de spawn deben referenciar el prefab correcto.

**Ejemplo de jerarquía:**
```
Enemy
├── ModeloVisual
```


### Objetos Destruibles

1. Crea un GameObject llamado `DestructibleObject`.
2. Añade los scripts:
   - `DestructibleHealthController`
   - `HealthComponentBehaviour`


### Armas (GameObject de arma en la escena, si aplica)

- Si el arma tiene presencia física, crea un GameObject `Weapon` y asígnale el script `WeaponHitbox`.
- Normalmente, las armas se gestionan desde ScriptableObjects y el sistema de inventario/equipamiento. **No asignes scripts de arma manualmente** salvo que implementes comportamientos avanzados.


### Áreas de Daño

1. Crea un GameObject llamado `DamageArea`.
2. Añade el script `AreaDamage`.
3. El script `AreaDamageTimer` se añade automáticamente cuando un objeto entra en el área.


### UI y Subsistemas

1. Crea GameObjects para la UI (por ejemplo, `InventoryUI`, `AchievementsUI`, `QuestsUI`).
2. Añade los scripts:
   - `InventoryUIUpdater` (en el GameObject de inventario)
   - `AchievementSystem` (en el GameObject de logros o uno global)
   - `QuestSystem` (en el GameObject de misiones o uno global)

      Campos y checks recomendados en el Inspector (UI):
      - `InventoryUIUpdater`:
         - `InventoryModel inventoryModel` (arrastrar el scriptable o GameObject que exponga el modelo)
         - `Transform slotContainer` (contenedor de slots)
         - `GameObject slotPrefab` (prefab para cada slot)
      - `AchievementSystem` / `QuestSystem`:
         - Verificar que se suscriban en `OnEnable` y se desuscriban en `OnDisable` a `GameEventBus`.

      Checklist:
      - Canvas activo en la escena.
      - Referencias a prefabs y modelos asignadas en el Inspector.


### GameManager y Sistemas Globales

1. Crea un GameObject vacío llamado `GameManager` en la raíz de la escena.
2. Añade los scripts:
   - `GameManager` (control global)
   - `GameEventBus` (se crea automáticamente si es necesario)
   - `GameServices` (se crea automáticamente si es necesario)

---


## 6. Ubicación de scripts en la jerarquía de Unity

> **Consejo:** Usa prefabs para guardar configuraciones y evitar tener que repetir el trabajo en cada escena.

- Los scripts del jugador van en el GameObject principal del prefab `Player`.
- Los scripts de enemigos y objetos destruibles van en sus respectivos prefabs.
- Los scripts de UI van en los GameObjects de la interfaz (Canvas, paneles, etc.).
- Los sistemas globales (GameManager, EventBus) van en GameObjects vacíos en la raíz de la escena.


## 7. Configuración de componentes y variables en el Inspector

- Haz clic en el GameObject al que asignaste un script.
- En el panel Inspector, verás los campos públicos y `[SerializeField]` de cada script.
- Ajusta los valores de vida, stamina, inventario y referencias a prefabs según la dificultad y el diseño.
- Asigna los ScriptableObjects y referencias necesarias en cada componente (arrastra desde el panel Project).

   Ejemplo de variables clave y su propósito:
   - `maxHealth` (float): vida máxima de la entidad. Si no está asignado, el valor por defecto es 100.
   - `regenRate` (float): tasa de regeneración por segundo.
   - `staminaCost` (int): coste de stamina por ataque.
   - `WeaponHitboxPrefab` (GameObject): prefab que contiene el `WeaponHitbox` y el collider para aplicar daño.

   Buenas prácticas al asignar:
   - Usa prefabs para los `Player` y `Enemy` con los componentes ya configurados.
   - Mantén una carpeta `Assets/ScriptableObjects/` con los `WeaponItem` y `HealingItem` para facilitar la asignación.

> **¿Qué es el Inspector?**
> Es el panel de Unity donde puedes ver y modificar las propiedades de los objetos seleccionados.


## 8. Flujo de datos persistentes entre escenas

- El estado del jugador (vida, stamina, inventario, etc.) se guarda automáticamente al cambiar de escena o morir.
- Se recupera al cargar la escena de exploración o combate usando el sistema `PlayerPersistentData`.
- No necesitas hacer nada manual: los scripts se encargan de guardar y restaurar los datos.


## 9. Consejos y buenas prácticas para principiantes

- Usa prefabs para reutilizar configuraciones y ahorrar tiempo.
- Organiza la jerarquía de la escena por tipo de entidad (jugador, enemigos, UI, sistemas globales).
- Usa ScriptableObjects para datos globales y configuraciones (por ejemplo, armas, datos persistentes).
- Prueba cada sistema por separado antes de integrarlo en la escena final.
- Haz copias de seguridad de tus escenas y prefabs antes de hacer cambios grandes.
- Lee los mensajes de la consola de Unity para detectar errores y advertencias.


## 10. Preguntas frecuentes y solución de problemas

- **¿Por qué mi jugador no se mueve?**
  - Verifica que el script de movimiento esté asignado y que el `InputActionAsset` esté configurado.
- **¿Por qué no se guarda la vida o el inventario?**
  - Asegúrate de que los componentes `HealthComponentBehaviour` y `PlayerInventory` estén asignados al jugador.
- **¿Por qué no veo la UI?**
  - Revisa que los scripts de UI estén en los GameObjects correctos y que el Canvas esté activo.
- **¿Por qué no funciona el daño?**
  - Verifica que los objetos tengan el script `HealthComponentBehaviour` y que los colliders estén configurados como triggers si es necesario.
- Consulta la consola de Unity para mensajes de error y advertencias.

---

> **Glosario rápido:**
> - **GameObject:** Cualquier objeto en la escena de Unity.
> - **Prefab:** Plantilla reutilizable de un GameObject.
> - **Script:** Código que da comportamiento a un GameObject.
> - **Inspector:** Panel donde configuras los componentes de un GameObject.
> - **ScriptableObject:** Archivo de datos reutilizable para configuraciones globales.
> - **Componente:** Script o módulo que se añade a un GameObject para darle funcionalidad.
