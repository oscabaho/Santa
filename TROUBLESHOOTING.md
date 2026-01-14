# 🔧 Guía de Troubleshooting

Soluciones a problemas comunes encontrados durante el desarrollo en el proyecto Santa.

---

## 📋 Índice

- [Combate](#combate)
- [Sistema de Guardado](#sistema-de-guardado)
- [UI y Addressables](#ui-y-addressables)
- [Build y Compilación](#build-y-compilación)
- [Errores de Editor](#errores-de-editor)

---

## Combate

### ❌ Error: "Combat doesn't start" (Combate no inicia)

**Síntoma**: Llamas a `StartCombat` pero nada ocurre, o la cámara no cambia.

**Causas y Soluciones**:

1. **Lista de participantes vacía o nula**
   - **Check**: Asegúrate de pasar una lista válida `List<GameObject>` con al menos el player y un enemigo.
   - **Log**: `GameLog.LogError("StartCombat called with null or empty participants list!")`

2. **Falta el componente `HealthComponentBehaviour`**
   - **Check**: Todos los participantes deben tener este componente para ser considerados válidos.
   - **Solución**: Agrega el componente al prefab del enemigo/jugador.

3. **Tags incorrectos**
   - **Check**: El jugador debe tener tag `Player`. Los enemigos deben tener tag `Enemy`.
   - **Solución**: Ajusta los tags en el inspector.

4. **Layer incorrecto en la cámara**
   - **Check**: Verifica que la cámara de combate (Cinemachine) esté en un layer que renderice a los combatientes.

### ❌ Error: "Ability no hace daño"

**Síntoma**: La animación se reproduce pero la vida no baja.

**Causas y Soluciones**:

1. **UpgradeService no inicializado**
   - **Check**: El daño base depende del `UpgradeService`. Si es null, devuelve valores default o 0.
   - **Solución**: Asegúrate de que `GameLifetimeScope` registre `UpgradeService`.

2. **Targeting Strategy incorrecto**
   - **Check**: Si usas `SingleEnemyTargeting` pero el target es `null`, la ability no se ejecuta.
   - **Solución**: Verifica que se esté pasando un target válido a `SubmitPlayerAction`.

3. **Defensa del enemigo muy alta**
   - **Check**: Si `Defense >= Damage`, el daño resultante puede ser 0 o 1.
   - **Log**: Revisa el combat log para ver el valor final calculado.

### ❌ Error: "Acciones desordenadas o turnos saltados"

**Síntoma**: Un enemigo actúa dos veces o antes que el jugador cuando no debería.

**Causas y Soluciones**:

1. **Action Speed mal configurado**
   - **Check**: El orden se basa en `ActionSpeed`. Valores más altos = turno antes.
   - **Solución**: Ajusta `ActionSpeed` en el Ability asset.

2. **Race condition en `ExecuteTurnAsync`**
   - **Check**: Si una acción no espera (await) correctamente a que termine la animación, la siguiente puede empezar antes.
   - **Solución**: Asegúrate de usar `await UniTask.Delay(...)` o esperar el evento de fin de animación.

---

## Sistema de Guardado

### ❌ Error: "Save data not found" al cargar

**Síntoma**: `TryLoad` devuelve false aunque acabas de guardar.

**Causas y Soluciones**:

1. **Encryption Key incorrecta**
   - **Check**: Si cambiaste la constante `ENCRYPTION_KEY` en `SecureStorageService`, los saves antiguos no se podrán leer.
   - **Solución**: Restaura la clave original o borra los datos persistentes (`SaveService.Delete()`).

2. **Error de permisos de archivo**
   - **Check**: En móviles, asegúrate de tener permisos de escritura. En Editor, verifica que la carpeta exista.
   - **Path**: `Application.persistentDataPath`

3. **Validación de Versión fallida**
   - **Check**: Si `SaveData.Version` no coincide con `SAVE_VERSION` actual y no hay lógica de migración.
   - **Log**: `GameLog.LogWarning("Save version mismatch...")`

### ❌ Error: "Datos corruptos o nulos después de cargar"

**Síntoma**: El juego carga pero el nivel es 1 o la posición es (0,0,0).

**Causas y Soluciones**:

1. **Contributor no registrado**
   - **Check**: El componente `ISaveContributor` debe estar activo en la escena cuando se llama a `ReadContributors`.
   - **Solución**: Asegúrate de que los objetos a cargar ya están instanciados.

2. **Orden de ejecución**
   - **Check**: `TryLoad` debe llamarse DESPUÉS de que la escena esté lista.
   - **Solución**: Llama a cargar en `Start()` o via evento, no en `Awake()`.

---

## UI y Addressables

### ❌ Error: "Panel UI no aparece"

**Síntoma**: `ShowPanel` se llama pero la pantalla sigue igual.

**Causas y Soluciones**:

1. **Addressable Key incorrecto**
   - **Check**: Verifica `AddressableKeys.cs` vs la dirección real en el grupo de Addressables.
   - **Error común**: Typos como "UI_Panel_Combat" vs "UI_Combat_Panel".

2. **Addressables no construidos**
   - **Check**: Si hiciste cambios en assets, necesitas reconstruir.
   - **Solución**: `Window > Asset Management > Addressables > Groups > Build > New Build > Default Build Script`.

3. **Panel desactivado internamente**
   - **Check**: Algunos paneles tienen un `CanvasGroup` con alpha 0.
   - **Solución**: Verifica la animación de entrada del panel.

### ❌ Error: "Exception: AssetReference not found"

**Síntoma**: Error rojo en consola al intentar cargar algo.

**Causas y Soluciones**:

1. **Asset no marcado como Addressable**
   - **Check**: Selecciona el prefab y marca el checkbox "Addressable" en el inspector.
   - **Solución**: Agrégalo al grupo "UI_Panels" (o el que corresponda).

2. **Labels incorrectos**
   - **Check**: Si usas carga por Label, verifica que el asset tenga el label correcto.

---

## Build y Compilación

### ❌ Error: "Shader compiler errors" en Build

**Síntoma**: La build falla o objetos se ven rosados.

**Causas y Soluciones**:

1. **URP no configurado en Project Settings**
   - **Check**: `Project Settings > Graphics > Scriptable Render Pipeline Settings` debe apuntar al asset de URP.

2. **Shaders no incluidos**
   - **Check**: `Project Settings > Graphics > Always Included Shaders`.

### ❌ Error: "VContainer resolution failed"

**Síntoma**: Runtime exception `VContainerException: No registration for type...`.

**Causas y Soluciones**:

1. **Dependencia no registrada**
   - **Check**: Revisa `GameLifetimeScope.Configure()`.
   - **Solución**: Agrega `builder.Register<Service>(Lifetime.Singleton);`.

2. **Circular Dependency**
   - **Check**: Servicio A inyecta B, y B inyecta A.
   - **Solución**: Refactoriza para extraer una interfaz común o usa inyección diferida (Lazy).

---

## Errores de Editor

### ❌ Error: "Collision con EnemyTarget no detectada"

**Síntoma**: Click en el enemigo no selecciona nada.

**Causas y Soluciones**:

1. **Physics Raycaster faltante**
   - **Check**: La cámara principal debe tener un `PhysicsRaycaster` si usas clicks 3D.

2. **Layer Mask incorrecta**
   - **Check**: El código del raycast está filtrando por layers específicos.
   - **Solución**: Asegúrate de que el enemigo esté en el layer "Enemy" o "Interactable".

3. **Collider inactivo**
   - **Check**: `EnemyTarget` desactiva/activa su collider según la fase.
   - **Solución**: Verifica que estemos en fase `Targeting`.

---

## Cómo Reportar un Nuevo Bug

Si encuentras un error no listado aquí:

1. **Logs**: Copia el Stack Trace completo.
2. **Contexto**: ¿Qué hacías justo antes? (Pasos de reproducción).
3. **Entorno**: ¿Editor o Build? ¿Dispositivo?
4. **Crea un Issue** en GitHub usando el template de Bug Report (ver `CONTRIBUTING.md`).
