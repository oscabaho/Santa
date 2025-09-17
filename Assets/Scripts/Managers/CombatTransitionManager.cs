using UnityEngine;

/// <summary>
/// Manages the visual transition between the exploration state and the combat state.
/// </summary>
public class CombatTransitionManager : MonoBehaviour
{
    public static CombatTransitionManager Instance { get; private set; }

    [Header("Scene References")]
    [Tooltip("The main camera used for exploration.")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("The player GameObject in the exploration scene.")]
    [SerializeField] private GameObject explorationPlayer;

    private CombatEncounter _currentEncounter;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartCombat(CombatEncounter encounter)
    {
        if (encounter == null) return;
        _currentEncounter = encounter;

        GameStateManager.StartCombat();

        // Disable exploration elements
        if (mainCamera != null) mainCamera.gameObject.SetActive(false);
        if (explorationPlayer != null) explorationPlayer.GetComponent<Movement>().enabled = false;

        // Enable combat elements
        if (_currentEncounter.CombatCamera != null) _currentEncounter.CombatCamera.gameObject.SetActive(true);
        if (_currentEncounter.CombatSceneParent != null) _currentEncounter.CombatSceneParent.SetActive(true);

        // Start the battle logic
        TurnBasedCombatManager.Instance.StartCombat(_currentEncounter.CombatParticipants);
    }

    public void EndCombat()
    {
        if (_currentEncounter == null) return;

        // Disable combat elements
        if (_currentEncounter.CombatCamera != null) _currentEncounter.CombatCamera.gameObject.SetActive(false);
        if (_currentEncounter.CombatSceneParent != null) _currentEncounter.CombatSceneParent.SetActive(false);

        // Enable exploration elements
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);
        if (explorationPlayer != null) explorationPlayer.GetComponent<Movement>().enabled = true;

        GameStateManager.EndCombat();

        _currentEncounter = null;
    }
}
