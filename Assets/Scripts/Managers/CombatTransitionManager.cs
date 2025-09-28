using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the visual transition between the exploration state and the combat state.
/// </summary>
public class CombatTransitionManager : MonoBehaviour, ICombatTransitionService
{
    // Reduce visibility to internal to avoid external code depending on the concrete singleton.
    internal static CombatTransitionManager Instance { get; private set; }

    [Header("Scene References")]
    [Tooltip("The camera GameObject used for exploration.")]
    [SerializeField] private GameObject explorationCamera;
    [Tooltip("The player GameObject in the exploration scene.")]
    [SerializeField] private GameObject explorationPlayer;

    private GameObject _currentCombatSceneParent;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Register service interface for decoupled access
        ServiceLocator.Register<ICombatTransitionService>(this);
    }

    private void OnDestroy()
    {
        var registered = ServiceLocator.Get<ICombatTransitionService>();
        if ((UnityEngine.Object)registered == (UnityEngine.Object)this)
            ServiceLocator.Unregister<ICombatTransitionService>();
        if (Instance == this) Instance = null;
    }

    public void StartCombat(GameObject combatSceneParent)
    {
        _currentCombatSceneParent = combatSceneParent;

        var gameState = ServiceLocator.Get<IGameStateService>();
        if (gameState != null)
        {
            gameState.StartCombat();
        }
        else
        {
            Debug.LogError("CombatTransitionManager: IGameStateService not available to start combat. Ensure GameStateManager is present and registered.");
        }

        // Disable exploration elements
        if (explorationCamera != null) explorationCamera.SetActive(false);
        if (explorationPlayer != null) explorationPlayer.GetComponent<Movement>().enabled = false;

        // Enable combat elements
        if (_currentCombatSceneParent != null) _currentCombatSceneParent.SetActive(true);

        // Get the combatants from the arena AFTER activating the scene
        CombatArena arena = _currentCombatSceneParent.GetComponent<CombatArena>();
        if (arena == null)
        {
            Debug.LogError("CombatTransitionManager: No CombatArena component found on the CombatSceneParent.");
            // Also re-enable exploration elements to avoid getting stuck
            if (explorationCamera != null) explorationCamera.SetActive(true);
            if (explorationPlayer != null) explorationPlayer.GetComponent<Movement>().enabled = true;
            return;
        }

        // Start the battle logic
        var combatService = ServiceLocator.Get<ICombatService>();
        if (combatService != null)
        {
            combatService.StartCombat(arena.Combatants);
        }
        else
        {
            Debug.LogError("A ICombatService is required in the scene to start combat!");
        }

        // Switch to combat UI
        UIManager.Instance.ShowCombatUI();
    }

    public void EndCombat()
    {
        if (_currentCombatSceneParent == null) return;

        // Disable combat elements
        _currentCombatSceneParent.SetActive(false);

        // Enable exploration elements
        if (explorationCamera != null) explorationCamera.SetActive(true);
        if (explorationPlayer != null) explorationPlayer.GetComponent<Movement>().enabled = true;

        var gameState = ServiceLocator.Get<IGameStateService>();
        if (gameState != null)
        {
            gameState.EndCombat();
        }
        else
        {
            Debug.LogError("CombatTransitionManager: IGameStateService not available to end combat. Ensure GameStateManager is present and registered.");
        }

        _currentCombatSceneParent = null;

        // Switch to exploration UI
        UIManager.Instance.ShowExplorationUI();
    }
}