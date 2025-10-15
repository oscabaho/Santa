using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the visual transition between the exploration state and the combat state.
/// </summary>
public class CombatTransitionManager : MonoBehaviour, ICombatTransitionService
{
    // Reduce visibility to internal to avoid external code depending on the concrete singleton.
    private static CombatTransitionManager Instance { get; set; }

    [Header("Scene References")]
    [Tooltip("The camera GameObject used for exploration.")]
    [SerializeField] private GameObject explorationCamera;
    [Tooltip("The player GameObject in the exploration scene.")]
    [SerializeField] private GameObject explorationPlayer;
    [Tooltip("The player GameObject used for combat.")]
    [SerializeField] private GameObject combatPlayer;
    [Tooltip("The root GameObject for the exploration UI.")]
    [SerializeField] private GameObject explorationUI;
    [Tooltip("The root GameObject for the combat UI.")]
    [SerializeField] private GameObject combatUI;

    [Header("Transition Sequences")]
    [Tooltip("Sequence of tasks to execute when starting combat.")]
    [SerializeField] private TransitionSequence startCombatSequence;
    [Tooltip("Sequence of tasks to execute when ending combat.")]
    [SerializeField] private TransitionSequence endCombatSequence;

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
        if ((Object)registered == (Object)this)
            ServiceLocator.Unregister<ICombatTransitionService>();
        if (Instance == this) Instance = null;
    }

    public void StartCombat(GameObject combatSceneParent)
    {
        _currentCombatSceneParent = combatSceneParent;
    // Instance bookkeeping (pool management is handled by the caller).
        var context = BuildTransitionContext();
        if (startCombatSequence != null)
        {
            StartCoroutine(startCombatSequence.Execute(context));
        }
    }

    public void EndCombat()
    {
        if (_currentCombatSceneParent == null) return;

        var context = BuildTransitionContext();
        if (endCombatSequence != null)
        {
            StartCoroutine(endCombatSequence.Execute(context));
        }

        _currentCombatSceneParent = null;
    }

    private TransitionContext BuildTransitionContext()
    {
        var context = new TransitionContext();
        context.AddTarget(TargetId.ExplorationCamera, explorationCamera);
        context.AddTarget(TargetId.ExplorationPlayer, explorationPlayer);
        context.AddTarget(TargetId.CombatPlayer, combatPlayer);
        context.AddTarget(TargetId.ExplorationUI, explorationUI);
        context.AddTarget(TargetId.CombatUI, combatUI);
        context.AddTarget(TargetId.CombatSceneParent, _currentCombatSceneParent);
        return context;
    }
}