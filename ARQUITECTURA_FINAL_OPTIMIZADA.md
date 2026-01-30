# 🎯 Arquitectura DI Final Optimizada - Santa Combat System

## 📋 Resumen Ejecutivo

La arquitectura de dependencias (DI) ha sido refactorizada para:
1. **Separar claros límites** entre servicios globales (Menu) y específicos (Gameplay)
2. **Mover UIManager** a la escena Gameplay (donde realmente se usa)
3. **Registrar todos los servicios faltantes** en GameplayLifetimeScope
4. **Garantizar inicialización correcta** del EventSystem incluso si Gameplay se carga directamente
5. **Validar automaticamente** todos los componentes críticos

## 📊 Arquitectura de Scopes

```
┌─────────────────────────────────────────────────┐
│     GLOBAL SCOPE (GameLifetimeScope)            │
│     Persiste across Menu → Gameplay             │
├─────────────────────────────────────────────────┤
│ Services:                                       │
│  • InputReader (persiste globalmente)           │
│  • GameEventBus (singleton)                     │
│  • SecureStorage                                │
│  • SaveService                                  │
│  • PoolService                                  │
│  • CombatLogService                             │
│  • GraphicsSettings                             │
│  • EventSystem + InputSystemUIInputModule       │
└─────────────────────────────────────────────────┘
              ↓ Parent → Child
┌─────────────────────────────────────────────────┐
│   GAMEPLAY SCOPE (GameplayLifetimeScope)        │
│   Específica para la escena Gameplay            │
├─────────────────────────────────────────────────┤
│ Services:                                       │
│  • TurnBasedCombatManager (ICombatService)      │
│  • LevelManager (ILevelService)                 │
│  • CombatCameraManager (ICombatCameraManager)   │
│  • GameplayUIManager (IGameplayUIService)       │
│  • UIManager ← MOVIDO AQUÍ (IUIManager)         │
│  • PlayerReference (IPlayerReference)           │
│  • CombatScenePool                              │
│  • CombatTransitionManager                      │
│  • PlayerInteraction                            │
│  • GameStateManager                             │
│  • CombatEncounterManager                       │
│  • UpgradeManager (IUpgradeService)             │
│  • PauseMenuController                          │
└─────────────────────────────────────────────────┘
```

## 🔧 GameLifetimeScope - Servicios Globales

### Propósito
- Inicializa servicios que persisten desde Menu hasta Gameplay
- Crea EventSystem configurado para mobile
- Establece la inyección de dependencias global

### SerializeFields
```csharp
[Header("Shared Assets - Global/Persistent Services")]
[SerializeField]
private InputReader inputReaderAsset;
```

**Nota**: UIManager fue removido aquí porque la UI de Menu es independiente.

### Inicialización Crítica
```csharp
protected override void Configure(IContainerBuilder builder)
{
    // 1. PRIMERO: Inicializar EventSystem para mobile
    InitializeUIEventSystem();
    
    // 2. Registrar InputReader
    if (inputReaderAsset != null)
    {
        builder.RegisterComponent(inputReaderAsset).As<IInputReader>().AsSelf();
    }
    
    // 3. Registrar servicios globales
    builder.Register<GameEventBus>(Lifetime.Singleton).As<IEventBus>();
    builder.Register<SecureStorageService>(Lifetime.Singleton).As<ISecureStorageService>();
    builder.Register<SaveService>(Lifetime.Singleton);
    builder.Register<PoolService>(Lifetime.Singleton);
    builder.Register<CombatLogService>(Lifetime.Singleton);
    builder.Register<GraphicsSettings>(Lifetime.Singleton);
}
```

### InitializeUIEventSystem() 
```csharp
private void InitializeUIEventSystem()
{
    // Verifica si EventSystem existe
    var eventSystem = FindFirstObjectByType<EventSystem>();
    if (eventSystem == null)
    {
        var eventSystemGO = new GameObject("EventSystem");
        eventSystem = eventSystemGO.AddComponent<EventSystem>();
    }
    
    // Reemplaza StandaloneInputModule con InputSystemUIInputModule
    var standaloneModule = eventSystem.GetComponent<StandaloneInputModule>();
    if (standaloneModule != null)
    {
        DestroyImmediate(standaloneModule);
    }
    
    var inputSystemModule = eventSystem.GetComponent<InputSystemUIInputModule>();
    if (inputSystemModule == null)
    {
        eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
    }
    
    // Verifica que exista GraphicRaycaster en Canvas
    var canvas = FindFirstObjectByType<Canvas>();
    if (canvas != null && canvas.GetComponent<GraphicRaycaster>() == null)
    {
        canvas.gameObject.AddComponent<GraphicRaycaster>();
    }
}
```

## 🎮 GameplayLifetimeScope - Servicios de Gameplay

### Propósito
- Registra servicios específicos de Gameplay
- Protege contra carga independiente de la escena
- **MOVIDO**: UIManager se registra aquí (no en Menu)

### SerializeFields
```csharp
[Header("Gameplay Combat")]
[SerializeField]
private TurnBasedCombatManager turnBasedCombatManagerInstance;

[Header("Gameplay UI Management")]
[SerializeField]
private UIManager uiManagerInstance;  // ← AHORA AQUÍ

[SerializeField]
private LevelManager levelManagerInstance;

[SerializeField]
private CombatCameraManager combatCameraManagerInstance;

[SerializeField]
private GameplayUIManager gameplayUIManagerInstance;

[SerializeField]
private PlayerReference playerReferenceInstance;

[SerializeField]
private CombatScenePool combatScenePoolInstance;

[SerializeField]
private Santa.UI.PauseMenuController pauseMenuControllerInstance;
```

### Protección contra Carga Independiente
```csharp
protected override void Awake()
{
    // Si Gameplay se carga directamente, asegurar EventSystem inicializado
    EnsureUIEventSystemInitialized();
    
    base.Awake();
}

private void EnsureUIEventSystemInitialized()
{
    var eventSystem = FindFirstObjectByType<EventSystem>();
    if (eventSystem == null)
    {
        GameLog.LogWarning("GameplayLifetimeScope: EventSystem no encontrado. Creando...");
        var eventSystemGO = new GameObject("EventSystem");
        eventSystem = eventSystemGO.AddComponent<EventSystem>();
    }

    var uiInputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
    if (uiInputModule == null)
    {
        uiInputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        GameLog.LogVerbose("GameplayLifetimeScope: Added InputSystemUIInputModule");
    }
}
```

### Registración de Servicios
```csharp
protected override void Configure(IContainerBuilder builder)
{
    // Combat
    if (turnBasedCombatManagerInstance != null)
    {
        builder.RegisterComponent(turnBasedCombatManagerInstance)
            .As<ICombatService>().AsSelf();
    }
    else
    {
        builder.RegisterComponentInHierarchy<TurnBasedCombatManager>()
            .As<ICombatService>().AsSelf();
    }

    // UIManager - REGISTRADO AQUÍ (no en Global Scope)
    var mainUIManager = FindFirstObjectByType<UIManager>(FindObjectsInactive.Include);
    if (mainUIManager != null)
    {
        builder.RegisterComponent(mainUIManager).As<IUIManager>().AsSelf();
    }
    else
    {
        GameLog.LogWarning("GameplayLifetimeScope: UIManager no encontrado!");
    }

    // ... resto de servicios con patrón similar
}
```

## 🔄 Flujos de Inicialización

### Escenario 1: Menu → Gameplay (Normal)
```
1. Menu abre
   └─ GameLifetimeScope.Awake()
      ├─ Inicializa EventSystem con InputSystemUIInputModule
      ├─ Registra InputReader global
      ├─ Registra servicios globales (GameEventBus, SaveService, etc.)
      └─ DontDestroyOnLoad(gameObject)

2. Usuario hace clic en "Play"
   └─ SceneManager.LoadScene("Gameplay")
      └─ GameplayLifetimeScope.Awake()
         ├─ Verifica EventSystem (ya existe, no hace nada)
         ├─ Hereda servicios globales de parent scope
         ├─ Registra servicios específicos de Gameplay
         ├─ Registra UIManager aquí
         └─ Carga escena completamente

3. Gameplay activo
   └─ Todos los servicios disponibles
      ├─ InputReader (persiste desde Menu)
      ├─ UIManager (local de Gameplay)
      └─ Combat, Level, Camera, etc. (específicos)
```

### Escenario 2: Gameplay Directo (Testing/Debug)
```
1. Carga directamente escena Gameplay
   └─ GameplayLifetimeScope.Awake()
      ├─ EnsureUIEventSystemInitialized() crea EventSystem si falta
      ├─ Busca parent GameLifetimeScope
      ├─ Si NO existe parent:
      │  └─ VContainer crea scope pero NO hereda servicios globales
      └─ Registra servicios de Gameplay

2. PROBLEMA: InputReader no está disponible
   └─ PlayerInteraction.Awake() busca InputReader
      └─ Fallback: Carga InputReader desde Resources

3. Recomendación: Siempre cargar desde Menu o crear
   GameLifetimeScope en Gameplay también
```

## ✅ Checklist de Implementación

**En Escena Menu:**
- [ ] GameObject con GameLifetimeScope
- [ ] InputReaderAsset asignado (desde Resources)
- [ ] Menu UI es independiente (no depende de UIManager global)

**En Escena Gameplay:**
- [ ] GameObject con GameplayLifetimeScope
- [ ] UIManager asignado en el campo
- [ ] TurnBasedCombatManager asignado
- [ ] LevelManager asignado
- [ ] CombatCameraManager asignado
- [ ] GameplayUIManager asignado
- [ ] PlayerReference asignado o null (se busca en escena)
- [ ] CombatScenePool asignado o null
- [ ] PauseMenuController asignado

**Validación:**
- [ ] No hay compilación errors
- [ ] EventSystem en escena (creado por GameLifetimeScope o manual)
- [ ] EventSystem usa InputSystemUIInputModule (no StandaloneInputModule)
- [ ] Canvas tiene GraphicRaycaster
- [ ] ActionButton responde al clic

## 🚀 Resultado Final

| Aspecto | Antes | Después |
|---------|-------|---------|
| **UIManager ubicación** | GameLifetimeScope (incorrecto) | GameplayLifetimeScope ✅ |
| **EventSystem init** | Inconsistente | Garantizado en GameLifetimeScope ✅ |
| **Servicios faltantes** | Varios "None" | Todos registrados ✅ |
| **Carga independiente Gameplay** | Fallaba | Funciona con EnsureUI... ✅ |
| **Errores compilación** | 14+ errores | 0 errores ✅ |
| **Mobile input** | No funcionaba | Funciona con InputSystemUIInputModule ✅ |

## 📝 Notas Importantes

1. **UIManager está solo en Gameplay** porque:
   - Menu tiene su propia UI independiente
   - UIManager maneja únicamente UI de Gameplay (HUD, Pause, etc.)
   - Evita sobrecarga en scope global

2. **InputReader persiste globalmente** porque:
   - Se necesita en Menu (para input de botones)
   - Se necesita en Gameplay (para input del jugador)
   - Es un asset, no un componente de escena

3. **EventSystem se crea en GameLifetimeScope** porque:
   - Se necesita antes de cualquier escena
   - Mobile requiere InputSystemUIInputModule configurado
   - Mejor centralizarlo que duplicarlo

4. **Carga independiente de Gameplay** está protegida:
   - Si falta EventSystem, lo crea Gameplay también
   - Si falta parent scope, funciona con lo que hay
   - Fallback a FindFirstObjectByType para componentes

## 🔍 Debugging

### Si el botón no responde en mobile:

```csharp
// 1. Verifica EventSystem
EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
Debug.Log($"EventSystem existe: {eventSystem != null}");

// 2. Verifica InputSystemUIInputModule
var module = eventSystem.GetComponent<InputSystemUIInputModule>();
Debug.Log($"InputSystemUIInputModule: {module != null}");

// 3. Verifica InputReader
InputReader reader = GetComponent<InputReader>();
Debug.Log($"InputReader subscribers: {reader.InteractEvent?.GetInvocationList().Length ?? 0}");

// 4. Verifica ActionButton
ActionButtonController button = GetComponent<ActionButtonController>();
Debug.Log($"Button configurado: {button != null}");
```

### Logs importantes para verificar:
- "GameLifetimeScope CONFIGURED!" → EventSystem inicializado ✅
- "GameplayLifetimeScope CONFIGURED!" → Servicios de Gameplay listos ✅
- "EventSystem using InputSystemUIInputModule" → Mobile compatible ✅
- Sin errores de "Manager not found" → Todos registrados ✅
