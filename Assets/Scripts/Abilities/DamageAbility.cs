using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A concrete Ability that deals a specified amount of damage to its targets.
/// The damage, cost, targeting, and speed are all configured on the asset.
/// </summary>
[CreateAssetMenu(fileName = "New Damage Ability", menuName = "Santa/Abilities/Damage Ability", order = 52)]
public class DamageAbility : Ability
{
    [Header("Damage Settings")]
    [SerializeField] private int _damage = 10;

    public override void Execute(List<GameObject> targets, GameObject caster)
    {
        if (targets == null) return;

    GameLog.Log($"{caster.name} uses {AbilityName}!");

        foreach (var target in targets)
        {
            if (target == null) continue;

            var healthComponent = target.GetComponent<HealthComponentBehaviour>();
            if (healthComponent != null)
            {
                healthComponent.AffectValue(-_damage);
                GameLog.Log($"{target.name} takes {_damage} damage.");
            }
        }
    }
}
