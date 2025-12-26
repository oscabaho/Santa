# Índice Rápido - Sistema de Guardado

Navegación rápida para encontrar información específica sobre el sistema de guardado.

## Acceso Rápido por Rol

### Soy Developer Trabajando en Guardados

1. **Necesito crear un nuevo componente con guardado:**
   - Consulta: `SaveContributorTemplate.cs`
   - Referencia: `SAVE_SYSTEM_GUIDE.md` → Sección "Implementación Básica"

2. **Necesito entender cómo funciona todo:**
   - Lee: `SAVE_SYSTEM_IMPLEMENTATION.md`
   - Luego: `SAVE_SYSTEM_GUIDE.md`

3. **Necesito debuggear un problema:**
   - Mira: `INSTALLATION_GUIDE.md` → Sección "Troubleshooting"
   - Luego: Logs esperados en consola

### Soy Technical Lead Revisando Arquitectura

- Referencia: `SAVE_SYSTEM_IMPLEMENTATION.md`
- Busca: Sección "Verificación Checklist"
- Componentes: Tabla de contribuyentes

---

## Conceptos Clave (30 segundos cada uno)

### ¿Qué es ISaveContributor?

Una interfaz que permite a componentes guardar/cargar datos:

```csharp
public interface ISaveContributor
{
    void WriteTo(ref SaveData data);
    void ReadFrom(in SaveData data);
}
```

### ¿Cómo se guarda el juego?

```text
SaveService.Save() → Todos los componentes WriteTo() → SaveData → Encriptación → Disco
```

### ¿Cómo se carga el juego?

```text
Archivo → Desencriptación → SaveData → Todos los componentes ReadFrom() → GameLoadedEvent
```

### ¿Dónde aparece el jugador al cargar?

En el **SpawnPoint** (Transform buscado en escena), no en la posición guardada.

---

## Flujo de Aprendizaje Recomendado

### Nivel 1: Conceptos Básicos (15 minutos)

1. Lee esta página (Índice)
2. Consulta diagrama en `SAVE_SYSTEM_IMPLEMENTATION.md`
3. Mira `SaveContributorTemplate.cs` (lectura pasiva)

### Nivel 2: Implementación Práctica (30 minutos)

1. Lee `SAVE_SYSTEM_GUIDE.md` completamente
2. Abre `SaveContributorTemplate.cs`
3. Crea tu primer componente basado en el template

### Nivel 3: Integración Completa (1 hora)

1. Lee `SAVE_SYSTEM_IMPLEMENTATION.md` - Secciones técnicas
2. Entiende `SaveService.cs` - Métodos principales
3. Implementa casos complejos (búsqueda, colecciones)

---

## Archivos por Propósito

### Implementación

- `SaveContributorTemplate.cs` - Plantilla lista para copiar
- `SaveService.cs` - Orquestador central
- `SaveData.cs` - Contenedor de datos
- `ISaveContributor.cs` - Interfaz base

### Guías

- `SAVE_SYSTEM_GUIDE.md` - Cómo implementar (práctica)
- `SAVE_SYSTEM_IMPLEMENTATION.md` - Arquitectura completa (teoría)
- `INSTALLATION_GUIDE.md` - Setup y testing

### Referencias

- `AddressableKeys.cs` - Claves de assets
- `GameEvents.cs` - GameLoadedEvent
- `SpawnPoint.cs` - Marcador de respawn

---

## Búsqueda Rápida

### Por palabra clave

| Busco | Ir a |
|-------|------|
| Guardar datos | SAVE_SYSTEM_GUIDE.md → Implementación Básica |
| Cargar datos | SAVE_SYSTEM_GUIDE.md → Integración con GameLoadedEvent |
| Componentes existentes | SAVE_SYSTEM_IMPLEMENTATION.md → Componentes Contribuyentes |
| Validación | SaveData.cs → Método Validate() |
| Encriptación | SecureStorageJson.cs → Métodos Encrypt/Decrypt |
| Respawn point | SpawnPoint.cs + SaveService.FindSpawnPoint() |
| Eventos | GameEvents.cs + SAVE_SYSTEM_GUIDE.md → Integración |

---

## Verificación Rápida

### ¿Todo está instalado correctamente?

```text
✓ SpawnPoint en escena con nombre "SpawnPoint"
✓ SaveService registrado en DI (GameLifetimeScope)
✓ IEventBus inyectado en SaveService
✓ Componentes implementan ISaveContributor
```

### ¿Debugging?

```text
Console → Busca "SaveService:"
Esperado: "Game saved successfully" + "Game loaded from:"
```

---

## Cheat Sheet - Código Mínimo

### Guardar

```csharp
_saveService.Save();
// Espera: "SaveService: Game saved successfully"
```

### Cargar

```csharp
if (_saveService.TryLoad(out var data))
{
    Debug.Log("Datos cargados: " + data.playerStats);
}
```

### Escuchar Carga

```csharp
_eventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);

private void OnGameLoaded(GameLoadedEvent e)
{
    Debug.Log("Jugador cargado en: " + e.SaveData.playerPosition);
}
```
