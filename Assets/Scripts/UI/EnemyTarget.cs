using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This component should be placed on any clickable enemy combatant.
/// It allows the player to select this enemy as a target for an ability.
/// It uses the IPointerClickHandler interface, which requires a PhysicsRaycaster on the camera
/// and an EventSystem in the scene running the InputSystemUIInputModule.
/// Selection only works while the combat system is in the Targeting phase.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EnemyTarget : MonoBehaviour, IPointerClickHandler
{
    private Collider _collider;
    private ICombatService _combatService;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        GameLog.Log($"EnemyTarget OnEnable on {gameObject.name}");
        // Always start with the collider disabled. It will be enabled by the combat manager.
        if (_collider != null) _collider.enabled = false;
    }

    private void Start()
    {
        // Start is the reliable place to find services registered in Awake.
        if (ServiceLocator.TryGet(out ICombatService svc))
        {
            _combatService = svc;
            // The manager now directly controls the collider, so no subscription is needed.
        }
        else
        {
            GameLog.LogWarning($"EnemyTarget on {gameObject.name} could not find ICombatService in Start. Clicks will not work.");
        }
    }

    private void OnDisable()
    {
        GameLog.Log($"EnemyTarget OnDisable on {gameObject.name}");
        if (_combatService != null)
        {
            // No subscription to remove.
            _combatService = null;
        }
    }

    public void SetColliderActive(bool isActive)
    {
        if (_collider != null)
        {
            _collider.enabled = isActive;
            GameLog.Log($"EnemyTarget on {gameObject.name}: Collider explicitly set to {isActive}.");
        }
        else
        {
            GameLog.LogWarning($"EnemyTarget on {gameObject.name}: Tried to set collider active, but _collider is null.");
        }
    }

    // Called by Unity's EventSystem when a pointer click hits this object's collider.
    public void OnPointerClick(PointerEventData eventData)
    {
        GameLog.Log($"EnemyTarget OnPointerClick on {gameObject.name}");
        TrySelect();
    }

    private void TrySelect()
    {
        GameLog.Log($"EnemyTarget TrySelect on {gameObject.name}");
        // By the time this runs the collider should already be disabled when clicks are invalid.
        // We still perform a defensive check to avoid unexpected behavior.
        if (!TurnBasedCombatManager.CombatIsInitialized)
        {
            GameLog.LogWarning("EnemyTarget clicked, but combat is not initialized. Ignoring.");
            return;
        }

        if (CombatUI.Instance == null)
        {
            GameLog.LogWarning("EnemyTarget: CombatUI.Instance is null.");
            return;
        }

        if (_combatService == null)
        {
            GameLog.LogWarning("EnemyTarget: ICombatService not found.");
            return;
        }

        if (_combatService.CurrentPhase != CombatPhase.Targeting)
        {
            // Not currently selecting targets, ignore clicks
            return;
        }

        // Notify the UI that this target has been selected
        CombatUI.Instance.OnTargetSelected(gameObject);
    }
}
