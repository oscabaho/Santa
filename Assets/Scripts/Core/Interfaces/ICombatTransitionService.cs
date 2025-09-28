using UnityEngine;

public interface ICombatTransitionService
{
    // Start combat given the GameObject parent that contains the combat scene visuals.
    void StartCombat(GameObject combatSceneParent);
    void EndCombat();
}