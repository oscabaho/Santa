using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Gain AP Ability", menuName = "Santa/Abilities/Gain AP Ability")]
public class GainAPAbility : Ability
{
    [Header("Effect")]
    [SerializeField] private int amountToGain = 50;

    public override void Execute(List<GameObject> targets, GameObject caster)
    {
        foreach (var target in targets)
        {
            var apComponent = target.GetComponent<ActionPointComponentBehaviour>();
            if (apComponent != null)
            {
                apComponent.AffectValue(amountToGain);
                GameLog.Log($"{target.name} used an ability to gain {amountToGain} AP.");
            }
        }
    }
}
