using UnityEngine;
using System.Collections.Generic;
using Santa.Domain.Combat;

/// <summary>
/// A simple component that holds a list of abilities for a combatant.
/// This list can be configured in the Inspector.
/// </summary>
public class AbilityHolder : MonoBehaviour
{
    [SerializeField]
    private List<Ability> _abilities = new List<Ability>();

    public List<Ability> Abilities => _abilities;
}
