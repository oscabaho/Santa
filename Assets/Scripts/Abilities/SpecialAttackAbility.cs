using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A special, powerful attack that costs energy and has a chance to miss.
/// Inherits base properties like AP cost, speed, and targeting from the Ability class.
/// </summary>
[CreateAssetMenu(fileName = "New Special Attack", menuName = "Santa/Abilities/Special Attack Ability", order = 53)]
public class SpecialAttackAbility : Ability
{
    [Header("Special Damage")]
    [SerializeField] private int _damage = 50;

    [Header("Special Properties")]
    [SerializeField] [Range(0f, 1f)] private float _missChance = 0.1f;

    public override void Execute(List<GameObject> targets, GameObject caster)
    {
        GameLog.Log($"{caster.name} attempts a Special Attack: {AbilityName}!");

        if (Random.value < _missChance)
        {
            GameLog.Log("...but it MISSED!");
            return;
        }

        if (targets == null) return;

    GameLog.Log("It's a direct hit!");
        foreach (var target in targets)
        {
            if (target == null) continue;

            var healthComponent = target.GetComponent<HealthComponentBehaviour>();
            if (healthComponent != null)
            { 
                healthComponent.AffectValue(-_damage);
                GameLog.Log($"{target.name} takes a massive {_damage} damage!");
            }
        }
    }
}
