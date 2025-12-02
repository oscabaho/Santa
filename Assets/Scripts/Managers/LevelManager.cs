using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Santa.Core.Pooling;
using UnityEngine;
using VContainer;

/// <summary>
/// Manages the game's level progression, including visual transformation of areas by instantiating prefabs.
/// </summary>
public class LevelManager : MonoBehaviour, ILevelService
{
    [Header("Level Data")]
    [Tooltip("List of all levels in the game.")]
    [SerializeField] private List<LevelData> levels;

    [Header("References")]
    [Tooltip("Optional parent transform for level visuals. If null, uses this GameObject's transform.")]
    [SerializeField] private Transform levelVisualsParent;

    private int currentLevelIndex = -1;
    private readonly List<GameObject> _activeGentrifiedVisuals = new List<GameObject>();
    private readonly List<GameObject> _activeLiberatedVisuals = new List<GameObject>();
    private Santa.Core.Save.EnvironmentDecorState _decorState;
    private IPoolService _pool;
    private readonly HashSet<LevelData> _prewarmedLevels = new();
    private CancellationTokenSource _levelChangeCancellation;

    [VContainer.Inject]
    public void Construct(IPoolService pool)
    {
        _pool = pool;
    }

    private void Start()
    {
        // Locate EnvironmentDecorState to persist liberation changes (optional, uses scene search)
        _decorState = FindFirstObjectByType<Santa.Core.Save.EnvironmentDecorState>(FindObjectsInactive.Include);
        if (_decorState == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("LevelManager: EnvironmentDecorState not found. Level liberation state won't persist across saves.");
#endif
        }
        if (levels != null && levels.Count > 0)
        {
            SetLevel(0).Forget();
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("LevelManager: No levels assigned in the inspector.");
#endif
        }
    }

    /// <summary>
    /// Gets the data for the current level.
    /// </summary>
    public LevelData GetCurrentLevelData()
    {
        if (currentLevelIndex >= 0 && currentLevelIndex < levels.Count)
        {
            return levels[currentLevelIndex];
        }
        return null;
    }

    /// <summary>
    /// Called after winning combat to transform the level to its 'liberated' state.
    /// </summary>
    public void LiberateCurrentLevel()
    {
        LevelData currentLevel = GetCurrentLevelData();
        if (currentLevel != null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"Liberating level: {currentLevel.levelName}");
#endif

            foreach (var visual in _activeGentrifiedVisuals)
            {
                if (visual != null) visual.SetActive(false);
            }
            foreach (var visual in _activeLiberatedVisuals)
            {
                if (visual != null) visual.SetActive(true);
            }

            // Record liberation change for save/load replay
            if (!string.IsNullOrEmpty(currentLevel.levelName))
            {
                _decorState?.ApplyChange($"liberated:{currentLevel.levelName}");
            }
        }
    }

    /// <summary>
    /// Called after the upgrade screen to progress to the next area.
    /// </summary>
    public void AdvanceToNextLevel()
    {
        int nextLevelIndex = currentLevelIndex + 1;
        if (levels != null && nextLevelIndex < levels.Count)
        {
            SetLevel(nextLevelIndex).Forget();
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("LevelManager: All levels have been liberated! Game Over.");
#endif
            // TODO: Handle game completion logic
        }
    }

    private async UniTaskVoid SetLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"LevelManager: Invalid level index {levelIndex}.");
#endif
            return;
        }

        // Cancel any in-progress level change
        _levelChangeCancellation?.Cancel();
        _levelChangeCancellation?.Dispose();
        _levelChangeCancellation = new CancellationTokenSource();
        var ct = _levelChangeCancellation.Token;

        try
        {
            // Destroy all visuals from the previous level before setting up the new one.
            if (currentLevelIndex != -1)
            {
                DestroyActiveVisuals();
            }

            currentLevelIndex = levelIndex;
            LevelData newLevel = levels[currentLevelIndex];

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"Setting up level: {newLevel.levelName}");
#endif

            // Prewarm and instantiate the initial 'gentrified' visuals for the new level asynchronously.
            if (_pool != null && !_prewarmedLevels.Contains(newLevel))
            {
                await PrewarmLevelVisualsAsync(newLevel);
                if (ct.IsCancellationRequested) return;
                _prewarmedLevels.Add(newLevel);
            }
            await InstantiateLevelVisualsAsync(newLevel);
            if (ct.IsCancellationRequested) return;

            // Proactively prewarm the next level to hide spikes
            PrewarmNextLevelIfAny();
        }
        catch (System.OperationCanceledException)
        {
            // Level change was cancelled, cleanup already happened
        }
        catch (System.Exception ex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogException(ex);
#endif
        }
    }

    private async UniTask InstantiateLevelVisualsAsync(LevelData levelData)
    {
        // Use the specified parent if available, otherwise use this manager's transform.
        Transform parent = levelVisualsParent != null ? levelVisualsParent : transform;

        int itemsProcessed = 0;
        const int batchSize = 5; // Process 5 items before yielding

        foreach (var prefab in levelData.gentrifiedVisuals)
        {
            if (prefab != null)
            {
                var instance = _pool != null
                    ? _pool.Get(prefab.name, prefab, parent.position, parent.rotation, parent)
                    : Instantiate(prefab, parent);
                _activeGentrifiedVisuals.Add(instance);
                itemsProcessed++;

                if (itemsProcessed % batchSize == 0)
                {
                    await UniTask.Yield();
                }
            }
        }
        foreach (var prefab in levelData.liberatedVisuals)
        {
            if (prefab != null)
            {
                var instance = _pool != null
                    ? _pool.Get(prefab.name, prefab, parent.position, parent.rotation, parent)
                    : Instantiate(prefab, parent);
                instance.SetActive(false);
                _activeLiberatedVisuals.Add(instance);
                itemsProcessed++;

                if (itemsProcessed % batchSize == 0)
                {
                    await UniTask.Yield();
                }
            }
        }
    }

    private async UniTask PrewarmLevelVisualsAsync(LevelData levelData)
    {
        // Build counts per prefab across both visual lists (avoid LINQ allocations)
        var countMap = new Dictionary<GameObject, int>(32);
        void AddCount(GameObject prefab)
        {
            if (prefab == null) return;
            if (countMap.TryGetValue(prefab, out var c))
            {
                countMap[prefab] = c + 1;
            }
            else
            {
                countMap[prefab] = 1;
            }
        }

        for (int i = 0; i < levelData.gentrifiedVisuals.Count; i++) AddCount(levelData.gentrifiedVisuals[i]);
        for (int i = 0; i < levelData.liberatedVisuals.Count; i++) AddCount(levelData.liberatedVisuals[i]);

        int processed = 0;
        foreach (var kv in countMap)
        {
            var prefab = kv.Key;
            int count = kv.Value;
            await _pool.PrewarmAsync(prefab.name, prefab, count);
            processed++;
            if ((processed & 3) == 3)
            {
                await UniTask.Yield();
            }
        }
    }

    private void PrewarmNextLevelIfAny()
    {
        if (_pool == null || levels == null) return;
        int next = currentLevelIndex + 1;
        if (next >= 0 && next < levels.Count)
        {
            var nextLevel = levels[next];
            if (!_prewarmedLevels.Contains(nextLevel))
            {
                PrewarmLevelVisualsAsync(nextLevel).Forget();
                _prewarmedLevels.Add(nextLevel);
            }
        }
    }

    private void DestroyActiveVisuals()
    {
        foreach (var visual in _activeGentrifiedVisuals)
        {
            if (visual != null)
            {
                if (_pool != null)
                {
                    _pool.Return(visual.name, visual);
                }
                else
                {
                    Destroy(visual);
                }
            }
        }
        _activeGentrifiedVisuals.Clear();

        foreach (var visual in _activeLiberatedVisuals)
        {
            if (visual != null)
            {
                if (_pool != null)
                {
                    _pool.Return(visual.name, visual);
                }
                else
                {
                    Destroy(visual);
                }
            }
        }
        _activeLiberatedVisuals.Clear();
    }
}
