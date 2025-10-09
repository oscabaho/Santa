using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the game's level progression, including visual transformation of areas.
/// </summary>
public class LevelManager : MonoBehaviour, ILevelService
{
    private static LevelManager Instance { get; set; }

    [Header("Level Configuration")]
    [Tooltip("The list of all levels/areas in the game, in order.")]
    [SerializeField] private List<LevelData> levels;

    private int currentLevelIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            ServiceLocator.Register<ILevelService>(this);
        }
    }

    private void OnDestroy()
    {
        var registered = ServiceLocator.Get<ILevelService>();
        if ((UnityEngine.Object)registered == (UnityEngine.Object)this)
            ServiceLocator.Unregister<ILevelService>();
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        if (levels != null && levels.Count > 0)
        {
            SetLevel(0);
        }
        else
        {
            GameLog.LogWarning("LevelManager: No levels assigned in the inspector.");
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
            GameLog.Log($"Liberating level: {currentLevel.levelName}");

            foreach (var visual in currentLevel.gentrifiedVisuals)
            {
                if (visual != null) visual.SetActive(false);
            }
            foreach (var visual in currentLevel.liberatedVisuals)
            {
                if (visual != null) visual.SetActive(true);
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
            GameLog.Log("LevelManager: All levels have been liberated! Game Over.");
            // TODO: Handle game completion logic
        }
    }

    private void SetLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            GameLog.LogError($"LevelManager: Invalid level index {levelIndex}.");
            return;
        }

        // Deactivate all visuals from the previous level before setting up the new one.
        if (currentLevelIndex != -1)
        {
            DeactivateAllVisuals(levels[currentLevelIndex]);
        }

        currentLevelIndex = levelIndex;
        LevelData newLevel = levels[currentLevelIndex];

    GameLog.Log($"Setting up level: {newLevel.levelName}");

        // Activate the initial 'gentrified' visuals for the new level.
        ActivateGentrifiedVisuals(newLevel);
    }

    private void ActivateGentrifiedVisuals(LevelData levelData)
    {
        foreach (var visual in levelData.gentrifiedVisuals)
        {
            if (visual != null) visual.SetActive(true);
        }
        foreach (var visual in levelData.liberatedVisuals)
        {
            if (visual != null) visual.SetActive(false);
        }
    }

    private void DeactivateAllVisuals(LevelData levelData)
    {
        foreach (var visual in levelData.gentrifiedVisuals)
        {
            if (visual != null) visual.SetActive(false);
        }
        foreach (var visual in levelData.liberatedVisuals)
        {
            if (visual != null) visual.SetActive(false);
        }
    }
}