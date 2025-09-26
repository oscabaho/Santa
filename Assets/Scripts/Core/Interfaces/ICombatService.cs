using System;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatService
{
    void StartCombat(List<GameObject> participants);
    IReadOnlyList<GameObject> Enemies { get; }
    void SubmitPlayerAction(Ability ability, GameObject primaryTarget);

    // Events to signal player turn lifecycle
    event Action OnPlayerTurnStarted;
    event Action OnPlayerTurnEnded;
}
