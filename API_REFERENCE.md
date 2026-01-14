# 📚 Referencia de API

Documentación técnica de las interfaces y clases principales del proyecto.

---

## Core Services

### ICombatService
Interfaz principal para interactuar con el sistema de combate.

**Implementación**: `TurnBasedCombatManager`
**Ubicación**: `Core/Interfaces/ICombatService.cs`

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `StartCombat(List<GameObject>)` | `void` | Inicializa un combate con los participantes dados. |
| `SubmitPlayerAction(Ability, GameObject)` | `void` | Registra la acción elegida por el jugador para el turno actual. |
| `CancelTargeting()` | `void` | Cancela el modo de selección de objetivo. |

### ISaveService
Sistema de persistencia de datos.

**Implementación**: `SaveService`
**Ubicación**: `Core/Interfaces/ISaveService.cs`

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `Save()` | `bool` | Guarda el estado actual de todos los contributors. Retorna éxito/fallo. |
| `TryLoad()` | `bool` | Intenta cargar el último save. Retorna true si tuvo éxito. |
| `Delete()` | `void` | Elimina permanentemente los datos guardados. |

### IUIManager
Gestor de interfaz de usuario basado en Addressables.

**Implementación**: `UIManager`
**Ubicación**: `Core/Interfaces/IUIManager.cs`

| Método | Retorno | Descripción |
|--------|---------|-------------|
| `ShowPanel(string)` | `UniTask<GameObject>` | Carga y muestra un panel UI de forma asíncrona. |
| `HidePanel(string)` | `void` | Oculta (desactiva) un panel previamente cargado. |

---

## Domain Objects

### Ability (ScriptableObject)
Clase base para todas las habilidades.

**Propiedades**:
- `string AbilityName`: Nombre para UI/Logs.
- `int ApCost`: Costo de uso.
- `TargetingStrategy Targeting`: Estrategia de selección.
- `int ActionSpeed`: Prioridad de turno.

**Métodos Abstractos**:
- `void Execute(...)`: Lógica principal del efecto.

### TargetingStrategy (ScriptableObject)
Define cómo se seleccionan los objetivos.

**Propiedades**:
- `TargetingStyle Style`: Enum (Self, SingleEnemy, AllEnemies, etc).

**Métodos Abstractos**:
- `void ResolveTargets(...)`: Popula la lista de targets válidos.

---

## Infrastructure

### SecureStorageService
Wrapper para acceso a disco encriptado.

**Interface**: `ISecureStorageService`

| Método | Descripción |
|--------|-------------|
| `Save<T>(key, data)` | Serializa y guarda data encriptada. |
| `TryLoad<T>(key, out data)` | Intenta cargar y deserializar. |

---

## Eventos (EventBus)

El proyecto usa un sistema de eventos desacoplado.

### Combat Events
- `CombatStartedEvent`: Al iniciar batalla.
- `CombatEndedEvent`: Al terminar (Win/Loss).
- `TurnStartedEvent`: Al inicio de cada turno.

### System Events
- `GameSavedEvent`: Después de guardar exitosamente.
- `GameLoadedEvent`: Después de cargar exitosamente.

---

**Última actualización:** Enero 2026
