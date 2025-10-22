using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the discrete phases of a combat encounter.
/// </summary>
public enum CombatPhase
{
    Selection,  // Players and AI are choosing their actions
    Targeting,  // The player is actively selecting a target
    Execution,  // Actions are being resolved in order
    Victory,    // The player has won the combat
    Defeat      // The player has lost the combat
}

public interface ICombatService
{
    // Properties
    GameObject Player { get; }
    IReadOnlyList<GameObject> AllCombatants { get; }
    IReadOnlyList<GameObject> Enemies { get; }
    CombatPhase CurrentPhase { get; }

    // Events
    event Action<CombatPhase> OnPhaseChanged;

    // Methods
    void StartCombat(List<GameObject> participants);
    void SubmitPlayerAction(Ability ability, GameObject primaryTarget = null);
}