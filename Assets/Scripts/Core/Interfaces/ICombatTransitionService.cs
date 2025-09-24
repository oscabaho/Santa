using UnityEngine;
using System.Collections.Generic;

public interface ICombatTransitionService
{
    // Start combat given the player, the list of participants (player + enemies),
    // and the GameObject parent that contains the combat scene visuals.
    void StartCombat(GameObject player, List<GameObject> participants, GameObject combatSceneParent);
    void EndCombat();
}
