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
    public void Construct(ICombatService combatService, CombatUI combatUI)
    {
        _combatService = combatService;
        _combatUI = combatUI;
    }

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

        if (_combatUI == null)
        {
            GameLog.LogWarning("EnemyTarget: CombatUI has not been injected.");
            return;
        }

        if (_combatService == null)
        {
            GameLog.LogWarning("EnemyTarget: ICombatService has not been injected.");
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
