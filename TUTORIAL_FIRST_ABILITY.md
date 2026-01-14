# 🎓 Tutorial: Crear tu Primera Ability

Tutorial paso a paso para crear una ability personalizada desde cero.

---

## 📋 Objetivo

Al final de este tutorial, habrás creado una **Freeze Ability** que:
- Congela a un enemigo por 1 turno
- Cuesta 2 Action Points
- Tiene targeting de enemigo único
- Muestra efectos visuales y mensajes en combat log

**Tiempo estimado:** 20-30 minutos

---

## Prerrequisitos

- ✅ Proyecto Santa configurado (ver [SETUP.md](SETUP.md))
- ✅ Unity Editor abierto
- ✅ Escena de combate funcionando

---

## Paso 1: Crear el ScriptableObject Script

### 1.1 Crear el archivo

Navega a: `Assets/Scripts/Domain/Combat/Abilities/`

**Click derecho** → `Create` → `C# Script`

**Nombre:** `FreezeAbility.cs`

### 1.2 Escribir el código

```csharp
using System.Collections.Generic;
using UnityEngine;
using Santa.Core;
using VContainer;

namespace Santa.Domain.Combat
{
    /// <summary>
    /// Ability that freezes an enemy for 1 turn, preventing them from acting.
    /// </summary>
    [CreateAssetMenu(fileName = "New Freeze Ability", menuName = "Santa/Abilities/Freeze Ability")]
    public class FreezeAbility : Ability
    {
        [Header("Freeze Settings")]
        [SerializeField] private int freezeDuration = 1;
        
        private static ICombatLogService _combatLog;
        
        [Inject]
        public void Construct(ICombatLogService combatLogService)
        {
            _combatLog = combatLogService;
        }
        
        public override void Execute(List<GameObject> targets, GameObject caster, 
            IUpgradeService upgradeService, IReadOnlyList<GameObject> allCombatants)
        {
            if (targets == null || targets.Count == 0)
            {
                GameLog.LogWarning("FreezeAbility: No targets provided.");
                return;
            }
            
            _combatLog?.LogMessage($"{caster.name} uses {AbilityName}!", CombatLogType.Info);
            
            // Apply freeze to all targets
            for (int i = 0; i < targets.Count; i++)
            {
                GameObject target = targets[i];
                if (target == null) continue;
                
                // Check if target has a brain (AI component)
                if (target.TryGetComponent<Brain>(out var brain))
                {
                    // Disable brain for the freeze duration
                    brain.enabled = false;
                    
                    // Log the freeze
                    _combatLog?.LogMessage($"{target.name} is frozen!", CombatLogType.Info);
                    
                    GameLog.Log($"FreezeAbility: {target.name} frozen for {freezeDuration} turn(s).");
                    
                    // TODO: Schedule re-enable after freeze duration
                    // This would require a status effect system
                }
                else
                {
                    GameLog.LogWarning($"FreezeAbility: {target.name} has no Brain component to freeze.");
                }
            }
        }
    }
}
```

### 1.3 Guardar y esperar compilación

**Guarda el archivo** (Ctrl+S) y espera a que Unity compile.

**Verifica**: No debe haber errores en la Console.

---

## Paso 2: Crear el Targeting Strategy Asset

Para esta ability necesitamos targeting de enemigo único.

### 2.1 Verificar si existe

Navega a: `Assets/Data/Combat/Targeting/`

**Busca:** `SingleEnemyTargeting` asset

### 2.2 Si no existe, créalo

**Click derecho** en la carpeta → `Create` → `Santa` → `Abilities` → `Targeting` → `Single Enemy`

**Nombre:** `SingleEnemyTargeting`

---

## Paso 3: Crear el Ability Asset

Ahora crearemos el asset de nuestra ability.

### 3.1 Navegar a la carpeta de abilities

`Assets/Data/Combat/Abilities/`

### 3.2 Crear el asset

**Click derecho** → `Create` → `Santa` → `Abilities` → `Freeze Ability`

**Nombre:** `Ability_Freeze`

### 3.3 Configurar en el Inspector

Selecciona el asset y configura:

```
┌─────────────────────────────────────┐
│ Freeze Ability (Script)             │
├─────────────────────────────────────┤
│ Info                                │
│   Ability Name: Freeze              │
│   Description: Freezes an enemy     │
│                for 1 turn           │
├─────────────────────────────────────┤
│ Properties                          │
│   Ap Cost: 2                        │
│   Targeting: SingleEnemyTargeting   │ ← Arrastra el asset aquí
│   Target Percentage: 1.0            │
│   Action Speed: 100                 │
├─────────────────────────────────────┤
│ Freeze Settings                     │
│   Freeze Duration: 1                │
└─────────────────────────────────────┘
```

**Importante:**
- ✅ **Ap Cost: 2** - Costo en Action Points
- ✅ **Targeting:** Arrastra `SingleEnemyTargeting` asset
- ✅ **Action Speed: 100** - Velocidad normal (mayor = más rápido)

---

## Paso 4: Asignar la Ability al Jugador

### 4.1 Abrir escena de combate

Abre: `Assets/Scenes/Combat/CombatArena_01.unity`

### 4.2 Seleccionar el Player

En la Hierarchy, busca: `Player` GameObject

### 4.3 Encontrar el AbilityHolder

En el Inspector, busca el componente `AbilityHolder` o `PlayerAbilities`

### 4.4 Agregar la ability

```
┌─────────────────────────────────────┐
│ Ability Holder (Script)             │
├─────────────────────────────────────┤
│ Abilities                           │
│   Size: 3                           │ ← Incrementa el tamaño
│   Element 0: Ability_BasicAttack    │
│   Element 1: Ability_Heal           │
│   Element 2: Ability_Freeze         │ ← Arrastra aquí
└─────────────────────────────────────┘
```

**Arrastra** el asset `Ability_Freeze` al slot vacío.

---

## Paso 5: Configurar UI Button (Opcional)

Si quieres un botón específico para la ability, necesitas configurar el CombatUI.

### 5.1 Abrir prefab de Combat UI

`Assets/Prefabs/UI/CombatUI.prefab`

### 5.2 Duplicar botón existente

En la Hierarchy dentro del prefab:
- Encuentra: `ActionButtons` → `Button_Attack`
- **Duplica** (Ctrl+D)
- **Renombra** a: `Button_Freeze`

### 5.3 Configurar el botón

En el componente `Button`:
- **Text**: Cambia a "Freeze"
- **Color**: Azul claro (opcional, para diferenciarlo)

En el componente `AbilityButton` (si existe):
- **Ability Index**: 2 (índice en el array de abilities)

**Guarda el prefab.**

---

## Paso 6: Probar la Ability

### 6.1 Entrar en Play Mode

**Click** en el botón ▶️ Play

### 6.2 Iniciar combate

Si no estás en combate automáticamente:
1. Mueve el jugador hacia un enemigo
2. Espera a que se active el combate

### 6.3 Usar la ability

1. **Verifica** que tienes al menos 2 AP
2. **Click** en el botón "Freeze"
3. **Click** en un enemigo para seleccionarlo como target
4. **Observa** el combat log: Debería decir "{Enemy} is frozen!"

### 6.4 Verificar el efecto

En el turno del enemigo:
- El enemigo congelado **NO debería** atacar
- Su Brain component está deshabilitado

---

## Paso 7: Añadir Efectos Visuales (Avanzado)

### 7.1 Crear VFX prefab (opcional)

Si tienes un sistema de VFX:

```csharp
[SerializeField] private string freezeVFXKey = "VFX_Freeze";

public override void Execute(...)
{
    // ... código existente ...
    
    // Spawn VFX
    if (_vfxService != null && !string.IsNullOrEmpty(freezeVFXKey))
    {
        _vfxService.PlayVFX(freezeVFXKey, target.transform.position);
    }
}
```

### 7.2 Agregar sonido (opcional)

```csharp
[SerializeField] private string freezeSoundKey = "SFX_Freeze";

// En Execute():
_audioService?.PlaySFX(freezeSoundKey);
```

---

## 🎉 ¡Completado!

Ahora tienes tu primera ability personalizada funcionando.

### ✅ Checklist de Verificación

- [ ] Script `FreezeAbility.cs` compilado sin errores
- [ ] Asset `Ability_Freeze` creado y configurado
- [ ] Targeting strategy asignada
- [ ] Ability asignada al jugador
- [ ] Botón de UI configurado (opcional)
- [ ] Ability probada en Play Mode
- [ ] Efecto visible en enemy (no ataca cuando congelado)

---

## 🔧 Troubleshooting

### Problema: "Ability no aparece en UI"

**Causa:** No está asignada al `AbilityHolder`

**Solución:** Verifica Paso 4, asegúrate de que está en el array.

### Problema: "Click en botón pero nada pasa"

**Causa posible:**
1. No tienes suficiente AP → Verifica que tienes 2+ AP
2. Targeting no asignado → Verifica el asset en Inspector

### Problema: "Enemy sigue atacando después de freeze"

**Causa:** El Brain no se está deshabilitando correctamente

**Debug:**
```csharp
// Agregar después de brain.enabled = false;
GameLog.Log($"Brain enabled: {brain.enabled}"); // Debería ser false
```

---

## 🚀 Próximos Pasos

Ahora que dominas lo básico, prueba:

1. **Modificar el costo de AP** - Hacer más barato/caro
2. **Cambiar el targeting** - Probar `AllEnemiesTargeting` para congelar a todos
3. **Agregar damage** - Combinar freeze con daño
4. **Implementar duración real** - Usar un sistema de status effects
5. **Crear más abilities** - Poison, Burn, Stun, etc.

---

## 📚 Ver También

- [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) - Documentación completa del sistema
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md) - Errores comunes y soluciones
- [TESTING.md](TESTING.md) - Cómo testear abilities

---

**Última actualización:** Enero 2026
