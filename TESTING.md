# 🧪 Guía de Testing

Estrategias y buenas prácticas para asegurar la calidad en el proyecto Santa.

---

## 📋 Niveles de Testing

1. [Unit Testing](#unit-testing) - Probar lógica aislada
2. [Integration Testing](#integration-testing) - Probar sistemas trabajando juntos
3. [Play Mode Testing](#play-mode-testing) - Probar comportamiento en runtime
4. [Manual Testing](#manual-testing) - Verificación humana

---

## Unit Testing

Usamos **NUnit** (incluido en Unity Test Framework) para pruebas unitarias.

### Ubicación
`Assets/Tests/Editor/`

### Ejemplo: Testear una Ability

Queremos verificar que `DamageAbility` calcula el daño correctamente.

```csharp
using NUnit.Framework;
using Santa.Domain.Combat;
using UnityEngine;

public class DamageAbilityTests
{
    [Test]
    public void CalculateDamage_ReturnsCorrectValue()
    {
        // Arrange
        // (En un entorno real, usaríamos Mocks para IUpgradeService)
        int baseStatsDamage = 10;
        int expectedDamage = 10;
        
        // Act
        // ... instanciar ability y ejecutar ...
        
        // Assert
        Assert.AreEqual(expectedDamage, baseStatsDamage);
    }
}
```

### Mocking con NSubstitute (Recomendado)

Para aislar dependencias como `IUpgradeService`:

```csharp
[Test]
public void Execute_AppliesDamageToTarget()
{
    // Arrange
    var upgradeService = Substitute.For<IUpgradeService>();
    upgradeService.DirectAttackDamage.Returns(20);
    
    var target = new GameObject("Target");
    target.AddComponent<HealthComponentBehaviour>();
    
    var ability = ScriptableObject.CreateInstance<DamageAbility>();
    
    // Act
    ability.Execute(new List<GameObject>{target}, null, upgradeService, null);
    
    // Assert
    var health = target.GetComponent<HealthComponentBehaviour>();
    Assert.Less(health.CurrentValue, health.MaxValue);
}
```

---

## Play Mode Testing

Usamos **Unity Test Runner** para probar lógica que requiere el engine (física, corrutinas).

### Ubicación
`Assets/Tests/PlayMode/`

### Ejemplo: Testear Respawn

```csharp
using UnityEngine.TestTools;
using System.Collections;
using NUnit.Framework;

public class RespawnTests
{
    [UnityTest]
    public IEnumerator PlayerRespawns_AtSpawnPoint()
    {
        // Arrange
        yield return SceneManager.LoadSceneAsync("CombatArena_01");
        var player = GameObject.FindGameObjectWithTag("Player");
        var initialPos = player.transform.position;
        
        // Act
        // ... simular muerte ...
        player.GetComponent<HealthComponentBehaviour>().Kill();
        
        yield return new WaitForSeconds(2.0f); // Esperar respawn
        
        // Assert
        Assert.AreNotEqual(initialPos, player.transform.position);
    }
}
```

---

## Integration Testing

Verificar que sistemas desconectados (SaveSystem + CombatSystem) funcionen bien.

### Checklist de Integración

**Save & Load en Combate:**
- [ ] Iniciar combate
- [ ] Intentar guardar (Debe fallar o estar deshabilitado)
- [ ] Terminar combate
- [ ] Guardar (Debe funcionar)
- [ ] Cargar (Debe restaurar estado post-combate)

**UI & Addressables:**
- [ ] Abrir menú (Load Async)
- [ ] Cerrar menú (Release)
- [ ] Abrir de nuevo (Load Async / Cache)
- [ ] Verificar memoria (No debe duplicarse)

---

## Manual Testing

### Sanity Check (Antes de cada commit)

1. **Build Check**: El proyecto compila sin errores.
2. **Start Game**: Iniciar desde Main Menu.
3. **Gameplay Loop**:
   - Entrar en combate
   - Ganar combate
   - Verificar rewards
4. **Save/Load**: Guardar, salir, volver a entrar, cargar.

### Performance Check (En dispositivo)

1. **FPS**: Mínimo 30 FPS estable (ideal 60).
2. **Memory**: No crashes por OOM (Out Of Memory).
3. **Load Times**: Transición de escenas < 5 segundos.

---

## CI/CD (Continuous Integration)

El proyecto está configurado para usar GitHub Actions (opcional).

### Workflow Típico
1. Push a `main` o PR
2. Trigger: Unity Build (Android/iOS/Windows)
3. Trigger: Run Editor Tests
4. Report: Success/Fail

---

## Debugging Tools Recomendadas

1. **RenderDoc**: Para debug de gráficos y shaders.
2. **Unity Profiler**: Para CPU/Memory spikes.
3. **Memory Profiler Package**: Para snapshots detallados de memoria.
4. **Console Log**: Usar `GameLog` (nuestro wrapper) para poder filtrar por categorías.

---

**Última actualización:** Enero 2026
