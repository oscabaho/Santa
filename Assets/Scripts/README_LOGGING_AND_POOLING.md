GameLog (centralised logging)

- Location: Assets/Scripts/Core/GameLog.cs
- Purpose: A thin wrapper around UnityEngine.Debug that is enabled in the Editor, Development builds, or when the GAME_LOGS_ENABLED symbol is defined.
- Usage: Call GameLog.Log(...), GameLog.LogWarning(...), GameLog.LogError(...), GameLog.LogFormat(...), GameLog.LogException(...)
- Why: Avoids accidental logging in release/mobile builds. To enable logs in release builds, define the GAME_LOGS_ENABLED symbol in Player Settings or project build configuration.

CombatScenePool and Addressables policy

- Location: Assets/Scripts/Core/CombatScenePool.cs and Assets/Scripts/Gameplay/CombatEncounter.cs
- Behavior:
  - The pool tries to reuse inactive instances keyed by a string (the encounter pool key).
  - If Addressables is available and the encounter has an address, the pool will instantiate via Addressables.InstantiateAsync and track the returned AsyncOperationHandle so it can ReleaseInstance properly.
  - Fallback: if Addressables not available or instantiation failed, the pool uses the fallback prefab or Resources path exposed in the encounter.
  - Per-encounter release policy: `CombatEncounter` exposes a flag `releaseAddressablesInstances` which tells the pool whether to call Addressables.ReleaseInstance when that instance is released back to the pool. This helps control memory usage for mobile devices.

- Best practices:
  - Prefer providing an Addressable entry for large combat scenes to leverage async loading and smaller initial APK sizes.
  - Use `CombatEncounter.autoPrewarm` and `prewarmCount` to create one or two instances at level load if you want to avoid a hitch on first combat. For mobile, `prewarmCount = 1` is usually a good compromise.
  - When creating new encounters, choose a stable `poolKey` that identifies the content; avoid dynamic keys that change each run.

Examples

- Instantiating a combat scene via the pool (simplified):

  StartCoroutine(CombatScenePool.Instance.GetInstanceAsync(encounter.GetPoolKey(), (go) => {
      if (go == null) { GameLog.LogError("Failed to create combat scene"); return; }
      // set parent, position and activate
      go.SetActive(true);
  }, encounter));

- Releasing an instance:

  CombatScenePool.Instance.ReleaseInstance(encounter.GetPoolKey(), instanceGameObject);

Notes

- Samples folder still contains Debug.Log calls; these are left untouched because they are third-party sample files and are not part of the production code path.
- We keep a thin compatibility wrapper `Assets/Scripts/Gameplay/GameLog.cs` (Obsolete) to ease migration. New code should use the Core `GameLog` directly.

If you'd like, I can add a small automated script to scan for Debug.* usages and optionally create a patch to migrate them across non-sample folders.