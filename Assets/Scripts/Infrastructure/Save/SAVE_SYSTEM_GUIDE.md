# Sistema de Guardado - Guía Completa

Documentación sobre cómo implementar persistencia de datos usando ISaveContributor.

## Conceptos Fundamentales

### Flujo de Guardado

```text
SaveService.Save()
  ↓
WriteContributors() - llamado por SaveService
  ↓
Todos los ISaveContributor.WriteTo(ref SaveData)
  ↓
Datos combinados en SaveData
  ↓
Encriptación + guardado a disco
```

### Flujo de Carga

```text
SaveService.TryLoad(out SaveData)
  ↓
Desencriptación + validación
  ↓
Posicionamiento del jugador en respawn point
  ↓
ReadContributors(in SaveData)
  ↓
Todos los ISaveContributor.ReadFrom(in SaveData)
  ↓
GameLoadedEvent publicado (notifica a otros sistemas)
```

## Implementación Básica

Paso 1: Tu clase debe implementar ISaveContributor

```csharp
public class MiComponente : MonoBehaviour, ISaveContributor
{
    private int miEstado;

    public void WriteTo(ref SaveData data)
    {
        // Guardar tu estado en data.extras
        AppendExtra(ref data, "MiComponente_Estado", miEstado.ToString());
    }

    public void ReadFrom(in SaveData data)
    {
        // Restaurar tu estado desde data.extras
        if (TryGetExtra(data, "MiComponente_Estado", out var value))
        {
            miEstado = int.Parse(value);
        }
    }

    private void AppendExtra(ref SaveData data, string key, string value)
    {
        var list = new System.Collections.Generic.List<SerializableKV>(
            data.extras ?? System.Array.Empty<SerializableKV>()
        );
        list.Add(new SerializableKV { key = key, value = value });
        data.extras = list.ToArray();
    }

    private bool TryGetExtra(in SaveData data, string key, out string value)
    {
        value = null;
        if (data.extras == null) return false;
        
        foreach (var kv in data.extras)
        {
            if (kv.key == key)
            {
                value = kv.value;
                return true;
            }
        }
        return false;
    }
}
```

## Implementación con Registro

Para mejor rendimiento, usa ISaveContributorRegistry:

```csharp
public class MiComponente : MonoBehaviour, ISaveContributor
{
    private ISaveContributorRegistry _registry;

    [VContainer.Inject]
    public void Construct(ISaveContributorRegistry registry)
    {
        _registry = registry;
    }

    private void OnEnable()
    {
        _registry?.Register(this);
    }

    private void OnDisable()
    {
        _registry?.Unregister(this);
    }

    public void WriteTo(ref SaveData data)
    {
        // Guardar datos
    }

    public void ReadFrom(in SaveData data)
    {
        // Cargar datos
    }
}
```

## Casos de Uso en el Proyecto

### Ya Implementados

#### 1. UpgradeManager

- Ubicación: `Presentation/Upgrades/UpgradeManager.cs`
- Guarda: `lastUpgrade`, `acquiredUpgrades[]`
- WriteTo: Copia lista de upgrades a SaveData
- ReadFrom: Restaura upgrades y reaplica estadísticas

#### 2. DefeatedEnemiesTracker

- Ubicación: `Infrastructure/Save/DefeatedEnemiesTracker.cs`
- Guarda: Enemigos derrotados por ID
- Escucha: CharacterDeathEvent
- Desactiva: Enemies cuando se cargan datos

#### 3. EnvironmentDecorState

- Ubicación: `Infrastructure/Save/EnvironmentDecorState.cs`
- Guarda: Cambios de decoración por ID
- Reaplica: Cambios al cargar (áreas liberadas)

### Requieren Implementación

1. Progreso de niveles/áreas
   - Qué nivel está desbloqueado
   - Qué área está completada

2. Estado del mapa/exploración
   - Zonas visitadas
   - Puertas abiertas

## Integración con GameLoadedEvent

Después de que SaveService.TryLoad() carga la partida, publica `GameLoadedEvent`.
Cualquier sistema que necesite reaccionar puede suscribirse:

```csharp
[Inject] private IEventBus _eventBus;

private void OnEnable()
{
    _eventBus?.Subscribe<GameLoadedEvent>(OnGameLoaded);
}

private void OnDisable()
{
    _eventBus?.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
}

private void OnGameLoaded(GameLoadedEvent e)
{
    // Aquí tienes acceso a los datos cargados en e.SaveData
    Debug.Log("Juego cargado exitosamente");
}
```

## Mejores Prácticas

- Siempre registra/desregistra en OnEnable/OnDisable
- Usa serialización simple (strings) en extras para máxima compatibilidad
- Valida datos antes de usarlos en ReadFrom
- Considera versionado si cambias la estructura de datos
