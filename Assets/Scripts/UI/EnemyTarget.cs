using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

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
    private CombatUI _combatUI;

    [Inject]
    public void Construct(ICombatService combatService)
    {
        _combatService = combatService;
        // CombatUI is loaded dynamically via Addressables, so we'll find it at runtime
    }

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"EnemyTarget OnEnable on {gameObject.name}");
#endif
        // Always start with the collider disabled. It will be enabled by the combat manager.
        if (_collider != null) _collider.enabled = false;
    }

    public void SetColliderActive(bool isActive)
    {
        if (_collider != null)
        {
            _collider.enabled = isActive;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"EnemyTarget on {gameObject.name}: Collider explicitly set to {isActive}.");
#endif
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning($"EnemyTarget on {gameObject.name}: Tried to set collider active, but _collider is null.");
#endif
        }
    }

    // Called by Unity's EventSystem when a pointer click hits this object's collider.
    public void OnPointerClick(PointerEventData eventData)
    {

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"EnemyTarget OnPointerClick on {gameObject.name}");
#endif
        TrySelect();
    }

    private void TrySelect()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"EnemyTarget TrySelect on {gameObject.name}");
#endif

        // Try to find CombatUI if not already set
        if (_combatUI == null)
        {
            _combatUI = FindAnyObjectByType<CombatUI>();
            if (_combatUI == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("EnemyTarget: CombatUI could not be found in the scene.");
#endif
                return;
            }
        }

        if (_combatService == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("EnemyTarget: ICombatService has not been injected.");
#endif
            return;
        }

        if (_combatService.CurrentPhase != CombatPhase.Targeting)
        {
            // Not currently selecting targets, ignore clicks
            return;
        }

        // Notify the UI that this target has been selected
        _combatUI.OnTargetSelected(gameObject);
    }
}
