using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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
    private readonly List<GameObject> _activeGentrifiedVisuals = new();
    private readonly List<GameObject> _activeLiberatedVisuals = new();
    private Santa.Core.Save.EnvironmentDecorState _decorState;

    private void Start()
    {
        // Locate EnvironmentDecorState to persist liberation changes
        _decorState = FindFirstObjectByType<Santa.Core.Save.EnvironmentDecorState>(FindObjectsInactive.Include);
        if (levels != null && levels.Count > 0)
        {
            SetLevel(0);
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
            if (!string.IsNullOrEmpty(currentLevel.levelName) && _decorState != null)
            {
                _decorState.ApplyChange($"liberated:{currentLevel.levelName}");
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
            SetLevel(nextLevelIndex);
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("LevelManager: All levels have been liberated! Game Over.");
#endif
            // TODO: Handle game completion logic
        }
    }

    private async void SetLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"LevelManager: Invalid level index {levelIndex}.");
#endif
            return;
        }

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

        // Instantiate the initial 'gentrified' visuals for the new level asynchronously.
        await InstantiateLevelVisualsAsync(newLevel);
    }

    private async Task InstantiateLevelVisualsAsync(LevelData levelData)
    {
        // Use the specified parent if available, otherwise use this manager's transform.
        Transform parent = levelVisualsParent != null ? levelVisualsParent : transform;
        int itemsProcessed = 0;
        int batchSize = 5; // Instantiate 5 items per frame to maintain 60 FPS

        foreach (var prefab in levelData.gentrifiedVisuals)
        {
            if (prefab != null)
            {
                var instance = Instantiate(prefab, parent);
                _activeGentrifiedVisuals.Add(instance);
                itemsProcessed++;

                if (itemsProcessed % batchSize == 0)
                {
                    await Task.Yield();
                }
            }
        }
        foreach (var prefab in levelData.liberatedVisuals)
        {
            if (prefab != null)
            {
                var instance = Instantiate(prefab, parent);
                instance.SetActive(false);
                _activeLiberatedVisuals.Add(instance);
                itemsProcessed++;

                if (itemsProcessed % batchSize == 0)
                {
                    await Task.Yield();
                }
            }
        }
    }

    private void DestroyActiveVisuals()
    {
        foreach (var visual in _activeGentrifiedVisuals)
        {
            if (visual != null) Destroy(visual);
        }
        _activeGentrifiedVisuals.Clear();

        foreach (var visual in _activeLiberatedVisuals)
        {
            if (visual != null) Destroy(visual);
        }
        _activeLiberatedVisuals.Clear();
    }
}
