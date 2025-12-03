# 🔧 Santa Project - Refactoring Checklist

## 📋 Objetivo

Este documento proporciona una lista de verificación práctica para estandarizar el código existente según las **Architecture Standards** del proyecto.

---

## 🎯 Prioridades de Refactoring

### ✅ Fase 1: Crítico (Completado)
- [x] Migración de `async void` → `async UniTaskVoid`
- [x] Eliminación de race conditions con CancellationToken
- [x] Manejo de excepciones en todos los métodos async
- [x] Corrección de warnings CS0414

### ✅ Fase 2: Namespaces (Completado)
- [x] Estandarizar namespaces en todos los archivos
- [x] Mover archivos a las carpetas correctas según su capa
- [x] Consolidar Save/Security bajo un solo namespace
- [x] Actualizar referencias en DI y servicios
- [x] **Conformidad: 100% (151/151 archivos)**

### ✅ Fase 3: Interfaces y Contratos (Completado)
- [x] Extraer interfaces embebidas a `_Core/Interfaces/`
  - [x] `IAIManager` → `_Core/Interfaces/IAIManager.cs`
  - [x] `IActionExecutor` → `_Core/Interfaces/IActionExecutor.cs`
- [x] Mover interfaces de ubicaciones incorrectas
  - [x] `ICombatEncounterManager` → `_Core/Interfaces/`
  - [x] `IBrain` → `_Core/Interfaces/`
  - [x] `ICombatEncounter` → `_Core/Interfaces/`
- [x] Agregar XML documentation a interfaces movidas
- [x] Revisar y documentar interfaces restantes
- [x] Agregar XML documentation completa a todas las interfaces
- [x] **Total interfaces organizadas: 30 en _Core/Interfaces/**
- [x] **Conformidad: 100% (151/151 archivos)**

### 🏗️ Fase 4: Separación de Capas
- [ ] Eliminar dependencias de Presentation → Domain
- [ ] Refactorizar clases que violan separación de capas
- [ ] Extraer lógica de negocio de MonoBehaviours

### ⚡ Fase 5: Optimización
- [ ] Eliminar LINQ restante en hot paths
- [ ] Implementar object pooling donde falta
- [ ] Optimizar allocations en combate

---

## 📝 Tareas Específicas por Archivo

### 🗂️ Managers Folder (Debe Moverse)

#### ❌ Problema
El folder `Assets/Scripts/Managers/` está en la raíz cuando debería estar organizado por capas.

#### ✅ Solución

**1. ICombatEncounterManager.cs**
```diff
- Ubicación actual: Assets/Scripts/Managers/ICombatEncounterManager.cs
+ Ubicación nueva: Assets/Scripts/_Core/Interfaces/ICombatEncounterManager.cs

+ namespace Santa.Core
+ {
      using Cysharp.Threading.Tasks;
      using UnityEngine;
      
      public interface ICombatEncounterManager
      {
          UniTask<bool> StartEncounterAsync(CombatEncounter encounter);
      }
+ }
```

**Implementación** debe estar en:
```
Assets/Scripts/Infrastructure/Combat/CombatEncounterManager.cs
```

---

### 🎮 Infrastructure Layer

#### 📁 Combat/TurnBasedCombatManager.cs

**Estado**: ✅ Mayormente correcto (ya refactorizado)

**Pendiente**:
```csharp
// ❌ Falta namespace
namespace Santa.Infrastructure.Combat
{
    // ... código existente
}
```

#### 📁 Audio/AudioManager.cs

**Revisar**:
- [ ] ¿Implementa `IAudioService`?
- [ ] ¿Usa object pooling?
- [ ] ¿Namespace correcto?

**Template esperado**:
```csharp
using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;

namespace Santa.Infrastructure.Audio
{
    public class AudioManager : MonoBehaviour, IAudioService
    {
        private ObjectPool<PooledAudioSource> _audioPool;
        
        [Inject]
        public void Construct(/* dependencies */)
        {
            // ...
        }
        
        public void PlaySFX(string soundKey, Vector3 position)
        {
            var source = _audioPool.Get();
            source.transform.position = position;
            source.Play(soundKey);
        }
    }
}
```

#### 📁 VFX/VFXManager.cs

**Estado**: ✅ Correcto (ya tiene namespace y pooling)

**Verificar**:
- [x] Namespace `Santa.Infrastructure.VFX` ✗ (falta agregar)
- [x] Implementa `IVFXService`
- [x] Usa `ObjectPool<T>`

---

### 🎨 Presentation Layer

#### 📁 Presentation/Managers/UIManager.cs

**Estado**: ⚠️ Namespace incorrecto

```diff
- // Sin namespace
+ namespace Santa.Presentation.UI
+ {
      using UnityEngine;
      using Cysharp.Threading.Tasks;
      using Santa.Core.Addressables;
      using VContainer;
      
      public class UIManager : MonoBehaviour, IUIManager
      {
          // ... código existente
      }
+ }
```

#### 📁 Presentation/Upgrades/UpgradeManager.cs

**Estado**: ⚠️ Sin namespace

```diff
+ namespace Santa.Presentation.Upgrades
+ {
      using UnityEngine;
      using VContainer;
      
      public class UpgradeManager : MonoBehaviour, IUpgradeService, IUpgradeTarget
      {
          // ... código existente
      }
+ }
```

#### 📁 Presentation/Menus/PauseMenuController.cs

**Estado**: ✅ Tiene namespace `Santa.UI`

**Verificar**:
- [x] Namespace correcto
- [x] Implementa `IPauseMenuService`
- [x] Usa UniTask

---

### 🏛️ Domain Layer

#### 📁 Domain/Player/PlayerReference.cs

**Estado**: ✅ Tiene namespace `Santa.Core.Player`

**Nota**: Namespace está en `Santa.Core.*` porque es un contrato fundamental, pero la implementación está en Domain. Esto es **correcto** según nuestra arquitectura.

#### 📁 Domain/Upgrades/UpgradeStrategySO.cs

**Estado**: ⚠️ Verificar namespace

```diff
+ namespace Santa.Domain.Upgrades
+ {
      using UnityEngine;
      
      public abstract class UpgradeStrategySO : ScriptableObject
      {
          // ... código existente
      }
+ }
```

**Todas las strategies** deben tener el mismo namespace:
- `IncreaseDamageStrategy.cs`
- `IncreaseMaxHealthStrategy.cs`
- `ReduceAPCostStrategy.cs`
- etc.

---

### 🧩 _Core Layer

#### 📁 _Core/Interfaces/

**Archivos que DEBEN estar aquí**:
```
✅ ICombatService.cs
✅ IUIManager.cs
✅ IPauseMenuService.cs
✅ IUpgradeService.cs
✅ IEventBus.cs
✅ IPoolService.cs
✅ ISaveService.cs
✅ IGameStateService.cs
✅ ILevelService.cs
✅ IVFXService.cs
✅ IAudioService.cs (⚠️ verificar si existe)
❌ ICombatEncounterManager.cs (DEBE MOVERSE desde Managers/)
```

**Verificar cada interface**:
```csharp
// Template correcto
namespace Santa.Core // O sub-namespace si aplica (Santa.Core.Audio)
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    
    /// <summary>
    /// Describe las operaciones del servicio de X
    /// </summary>
    public interface IServiceName
    {
        // Properties
        bool IsInitialized { get; }
        
        // Events
        event Action OnSomethingHappened;
        
        // Methods
        UniTask InitializeAsync();
        void DoSomething();
    }
}
```

#### 📁 _Core/Constants/

**Verificar**:
- [ ] `GameConstants.cs` - ¿Tiene namespace `Santa.Core.Config`?
- [ ] `AddressableKeys.cs` - ¿Namespace `Santa.Core.Addressables`?
- [ ] `UIStrings.cs` - ¿Namespace `Santa.Core.Config`?

```csharp
namespace Santa.Core.Config
{
    public static class GameConstants
    {
        public static class Tags
        {
            public const string Player = "Player";
            public const string Enemy = "Enemy";
        }
        
        public static class Layers
        {
            public const int Ground = 6;
            public const int Water = 4;
        }
        
        public static class PlayerPrefsKeys
        {
            public const string SfxVolume = "SfxVolume";
            public const string MusicVolume = "MusicVolume";
        }
    }
}
```

#### 📁 _Core/Events/

**Crear carpeta si no existe**: `Assets/Scripts/_Core/Events/`

**Mover todos los eventos aquí**:
```csharp
namespace Santa.Core.Events
{
    using UnityEngine;
    using System.Collections.Generic;
    
    public struct CombatStartedEvent
    {
        public List<GameObject> Participants;
        public CombatArena Arena;
    }
    
    public struct CharacterDeathEvent
    {
        public GameObject Character;
        public bool IsPlayer;
    }
    
    public struct CombatEndedEvent
    {
        public bool PlayerWon;
        public int ExperienceGained;
    }
}
```

---

## 🔄 Plan de Migración de Namespaces

### Paso 1: Auditoría

Ejecutar búsqueda de archivos sin namespace:

```powershell
# Buscar archivos C# sin namespace
Get-ChildItem -Path "Assets\Scripts" -Filter "*.cs" -Recurse | 
    Where-Object { (Get-Content $_.FullName -Raw) -notmatch "namespace\s+" } |
    Select-Object FullName
```

### Paso 2: Aplicar Namespaces por Capa

#### _Core Layer
```csharp
namespace Santa.Core                      // Interfaces generales
namespace Santa.Core.Pooling             // Pooling
namespace Santa.Core.Player              // Player contracts
namespace Santa.Core.Save                // Save system
namespace Santa.Core.Security            // Security/encryption
namespace Santa.Core.Config              // Constants, strings
namespace Santa.Core.Addressables        // Addressable keys
namespace Santa.Core.Events              // Event structs
namespace Santa.Core.Utils               // Utilities
```

#### Domain Layer
```csharp
namespace Santa.Domain.Combat            // Abilities, CombatAction
namespace Santa.Domain.Upgrades          // Upgrade strategies
namespace Santa.Domain.Entities          // Player/Enemy brains
namespace Santa.Domain.Dialogue          // Conversations
```

#### Infrastructure Layer
```csharp
namespace Santa.Infrastructure.Combat    // Combat managers
namespace Santa.Infrastructure.Audio     // Audio system
namespace Santa.Infrastructure.VFX       // VFX system
namespace Santa.Infrastructure.Save      // Save service
namespace Santa.Infrastructure.State     // Game state
namespace Santa.Infrastructure.Level     // Level manager
namespace Santa.Infrastructure.Camera    // Camera manager
namespace Santa.Infrastructure.Input     // Input reader
```

#### Presentation Layer
```csharp
namespace Santa.Presentation.UI          // UI managers
namespace Santa.Presentation.Combat      // Combat UI
namespace Santa.Presentation.Upgrades    // Upgrade UI
namespace Santa.Presentation.Menus       // Pause/settings menus
namespace Santa.Presentation.HUD         // HUD elements
```

#### UI Layer (Componentes Reutilizables)
```csharp
namespace Santa.UI                       // Generic UI components
```

### Paso 3: Actualizar Assembly Definitions

**Crear/Actualizar asmdef files**:

```json
// Santa.Core.asmdef
{
    "name": "Santa.Core",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

```json
// Santa.Domain.asmdef
{
    "name": "Santa.Domain",
    "references": ["Santa.Core"],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false
}
```

```json
// Santa.Infrastructure.asmdef
{
    "name": "Santa.Infrastructure",
    "references": ["Santa.Core", "Santa.Domain", "UniTask", "VContainer"],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false
}
```

```json
// Santa.Presentation.asmdef
{
    "name": "Santa.Presentation",
    "references": [
        "Santa.Core", 
        "Santa.Domain", 
        "Santa.Infrastructure", 
        "UniTask", 
        "VContainer",
        "Unity.InputSystem"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false
}
```

---

## 🧪 Testing de Cambios

### Después de cada refactoring:

1. **Compilación**: Verificar que no hay errores
```bash
# En Unity: Ctrl+R (Recompile)
```

2. **Tests Unitarios**: Ejecutar suite de tests
```bash
# Unity Test Runner: Window → General → Test Runner
```

3. **Test de Integración**: Verificar escenas principales
- [ ] TestScene se carga correctamente
- [ ] Combat flow funciona end-to-end
- [ ] UI dinámica carga vía Addressables
- [ ] Save/Load funciona

4. **Profiler**: Verificar que no hay regresiones de performance
- [ ] GC Allocations < 100 KB/frame en combate
- [ ] 60 FPS en dispositivo target

---

## 📊 Progreso de Refactoring

### Métricas

```
Fase 1 (Async/Await):        ████████████████████ 100% ✅
Fase 2 (Namespaces):         ████████████████████ 100% ✅
Fase 3 (Interfaces):         ████████████████████ 100% ✅
Fase 4 (Separación Capas):  ░░░░░░░░░░░░░░░░░░░░   0% ⏳
Fase 5 (Optimización):       ████████████░░░░░░░░  60% 🔄
```

### Archivos Restantes por Refactorizar

**✅ Namespaces: Completados** (100% conformidad alcanzado)

**Interfaces Restantes por Organizar**:
- [x] Mover interfaces restantes a `_Core/Interfaces/` ✅

---

## 🎯 Próximos Pasos

### Inmediato (Esta Semana)

1. [x] ~~Mover `ICombatEncounterManager` a `_Core/Interfaces/`~~ (No encontrado)
2. [x] ~~Agregar namespace a todos los archivos de `Infrastructure/Combat/`~~ ✅
3. [x] ~~Agregar namespace a todos los archivos de `Presentation/Upgrades/`~~ ✅
4. [x] ~~Actualizar `GameLifetimeScope.cs` con using statements correctos~~ ✅

### Corto Plazo (Este Mes)

1. [ ] Crear Assembly Definitions para cada capa
2. [x] ~~Agregar namespaces a toda la capa Domain~~ ✅
3. [x] ~~Reorganizar archivos según estructura definitiva~~ ✅
4. [ ] Actualizar documentación con nuevos namespaces

### Largo Plazo (Q1 2025)
1. [ ] Refactorizar violaciones de separación de capas
2. [ ] Implementar tests unitarios para cada servicio
3. [ ] Optimizar allocations restantes
4. [ ] Documentar APIs públicas con XML docs

---

## 📚 Referencias Rápidas

- **Architecture Standards**: `ARCHITECTURE_STANDARDS.md`
- **UniTask Guide**: `UNITASK_MIGRATION_GUIDE.md`
- **Addressables Setup**: `MANUAL_ADDRESSABLES_CONFIG.md`
- **Optimization Progress**: `ARCHITECTURE_OPTIMIZATION_PROGRESS.md`

---

## ✅ Checklist Diaria

Antes de terminar el día de trabajo:

- [ ] Todos los archivos nuevos tienen namespace correcto
- [ ] Interfaces en `_Core/Interfaces/`
- [ ] Código compila sin warnings
- [ ] No hay referencias `FindObjectOfType` en código de producción
- [ ] Todos los async son UniTask (no Task)
- [ ] CancellationToken en operaciones cancelables
- [ ] GameLog en lugar de Debug.Log
- [ ] Sin LINQ en hot paths

---

**Última actualización**: Diciembre 2025  
**Versión**: 1.0  
**Mantenedor**: Architecture Team
