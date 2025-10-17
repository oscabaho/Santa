using UnityEngine;

/// <summary>
/// This component should be placed on any clickable enemy combatant.
/// It allows the player to select this enemy as a target for an ability.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EnemyTarget : MonoBehaviour
{
    private void OnMouseDown()
    {
        // Check if the CombatUI instance exists and is ready
        if (CombatUI.Instance != null)
        {
            // Notify the UI that this target has been selected
            CombatUI.Instance.OnTargetSelected(gameObject);
        }
    }
}
