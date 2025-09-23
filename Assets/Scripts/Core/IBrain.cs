using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for any AI brain (enemies, allies), ensuring it has a method to choose an action.
/// </summary>
public interface IBrain
{
    PendingAction ChooseAction(
        PendingAction? playerAction,
        List<GameObject> allEnemies,
        List<GameObject> allAllies);
}
