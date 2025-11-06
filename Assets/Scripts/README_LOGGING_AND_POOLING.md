# Logging y Pooling

## GameLog (registro centralizado)

- Ubicación: `Assets/Scripts/Core/GameLog.cs`
- Propósito: Wrapper ligero sobre `UnityEngine.Debug` habilitado en Editor, builds de Development o cuando se define el símbolo `GAME_LOGS_ENABLED`.
- Uso: `GameLog.Log(...)`, `GameLog.LogWarning(...)`, `GameLog.LogError(...)`, `GameLog.LogFormat(...)`, `GameLog.LogException(...)`.
- Por qué: Evita logs accidentales en builds de release/mobile. Para habilitarlos en release, define `GAME_LOGS_ENABLED` en Player Settings o configuración de build.

## CombatScenePool y Addressables

- Ubicación: `Assets/Scripts/Core/CombatScenePool.cs` y `Assets/Scripts/Gameplay/CombatEncounter.cs`
- Comportamiento:
  - El pool reutiliza instancias inactivas agrupadas por una key (el address de Addressables).
  - **Las arenas de combate se configuran EXCLUSIVAMENTE mediante Addressables** usando `combatSceneAddress`.
  - El sistema instancia con `Addressables.InstantiateAsync` y guarda el `AsyncOperationHandle` para hacer `ReleaseInstance` correctamente.
  - Política por encuentro: `CombatEncounter` expone el flag `releaseAddressablesInstances` para decidir si llamar `Addressables.ReleaseInstance` al liberar. Útil para controlar memoria en mobile.

- Buenas prácticas:
  - **OBLIGATORIO:** Todas las arenas de combate deben estar marcadas como Addressables en Unity.
  - El campo `combatSceneAddress` debe contener el address exacto configurado en Addressables.
  - Usar `autoPrewarm = true` y `prewarmCount` para crear 1-2 instancias al cargar nivel y evitar hitch en el primer combate. En mobile, `prewarmCount = 1` es buen balance.
  - Para optimizar memoria en mobile, activar `releaseAddressablesInstances = true` en encuentros pesados o que no se repiten frecuentemente.

## Ejemplos

- Instanciar una escena de combate vía pool (simplificado):

```csharp
// En una clase donde se inyecta CombatScenePool
public class MyCombatClass : MonoBehaviour
{
    private CombatScenePool _combatScenePool;

    [Inject]
    public void Construct(CombatScenePool combatScenePool)
    {
        _combatScenePool = combatScenePool;
    }

    public async void LoadCombatScene(ICombatEncounter encounter)
    {
        var go = await _combatScenePool.GetInstanceAsync(encounter.GetPoolKey(), encounter);
        if (go == null) { GameLog.LogError("Failed to create combat scene"); return; }
        // set parent, position and activate
        go.SetActive(true);
    }

    public void ReleaseCombatScene(ICombatEncounter encounter, GameObject instanceGameObject)
    {
        _combatScenePool.ReleaseInstance(encounter.GetPoolKey(), instanceGameObject);
    }
}
```

- Liberar una instancia:

  `_combatScenePool.ReleaseInstance(encounter.GetPoolKey(), instanceGameObject);`

## Notas

- La carpeta Samples puede contener `Debug.Log`; se dejan intactos por ser terceros y no ruta de producción.
- Se mantiene un wrapper de compatibilidad `Assets/Scripts/Gameplay/GameLog.cs` (Obsoleto) para migración. Nuevo código debe usar el `GameLog` de Core.

Si quieres, puedo añadir un script para escanear usos de `Debug.*` y proponer un parche de migración en carpetas no-sample.
