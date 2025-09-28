using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseDamageStrategy", menuName = "Santa/Upgrades/Strategies/Increase Damage")]
public class IncreaseDamageStrategy : UpgradeStrategySO
{
    public AbilityType TargetAbility;
    public int DamageIncrease;

    public override void Apply(IUpgradeTarget target)
    {
        switch (TargetAbility)
        {
            case AbilityType.DirectAttack:
                target.IncreaseDirectAttackDamage(DamageIncrease);
                break;
            case AbilityType.AreaAttack:
                target.IncreaseAreaAttackDamage(DamageIncrease);
                break;
            case AbilityType.SpecialAttack:
                target.IncreaseSpecialAttackDamage(DamageIncrease);
                break;
        }
    }
}
