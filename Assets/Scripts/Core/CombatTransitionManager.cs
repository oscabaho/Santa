using UnityEngine;

/// <summary>
/// Manages the visual transition between the exploration state and the combat state.
/// This version is decoupled from scene references and discovers objects at runtime.
/// </summary>
public class CombatTransitionManager : MonoBehaviour, ICombatTransitionService
{
    [Header("Transition Sequences")]
    [Tooltip("Sequence of tasks to execute when starting combat.")]
    [SerializeField] private TransitionSequence startCombatSequence;
    [Tooltip("Sequence of tasks to execute when ending combat.")]
    [SerializeField] private TransitionSequence endCombatSequence;

    // --- Injected References ---
    private ScreenFade _screenFade;

    // --- Discovered References ---
    private GameObject _explorationCamera;
    private GameObject _explorationPlayer;
    
    // --- Runtime State ---
    private GameObject _currentCombatSceneParent;
    private TransitionContext _currentContext;

    [Inject]
    public void Construct(ScreenFade screenFade)
    {
        _screenFade = screenFade;
    }

    private void Awake()
    {
        // Discover persistent exploration objects
        _explorationPlayer = FindFirstObjectByType<ExplorationPlayerIdentifier>()?.gameObject;
        _explorationCamera = Camera.main?.gameObject;

        if (_explorationPlayer == null)
        {
            GameLog.LogError("CombatTransitionManager: Could not find the exploration player via the ExplorationPlayerIdentifier component.", this);
            enabled = false;
        }
        if (_explorationCamera == null)
        {
            GameLog.LogError("CombatTransitionManager: Could not find the main camera.", this);
            enabled = false;
        }
    }

    public void StartCombat(GameObject combatSceneParent)
    {
        _currentCombatSceneParent = combatSceneParent;

        // Discover combat-specific objects within the instantiated prefab
        var combatPlayer = _currentCombatSceneParent.GetComponentInChildren<CombatPlayerIdentifier>()?.gameObject;
        if (combatPlayer == null)
        {
            GameLog.LogError($"CombatTransitionManager: Could not find the combat player via the CombatPlayerIdentifier component within {_currentCombatSceneParent.name}.", this);
            return;
        }

        // Build and store the context for both start and end transitions
        _currentContext = new TransitionContext();
        _currentContext.AddTarget(TargetId.ExplorationCamera, _explorationCamera);
        _currentContext.AddTarget(TargetId.ExplorationPlayer, _explorationPlayer);
        _currentContext.AddTarget(TargetId.CombatPlayer, combatPlayer);
        _currentContext.AddTarget(TargetId.CombatSceneParent, _currentCombatSceneParent);
        _currentContext.AddToContext("ScreenFade", _screenFade);
        
        if (startCombatSequence != null)
        {
            StartCoroutine(startCombatSequence.Execute(_currentContext));
        }
    }

    public void EndCombat()
    {
        if (_currentCombatSceneParent == null || _currentContext == null)
        {
            GameLog.LogWarning("EndCombat was called but there is no active combat or context.", this);
            return;
        }

        if (endCombatSequence != null)
        {
            StartCoroutine(endCombatSequence.Execute(_currentContext));
        }

        // Clean up context after use
        _currentCombatSceneParent = null;
        _currentContext = null;
    }
}