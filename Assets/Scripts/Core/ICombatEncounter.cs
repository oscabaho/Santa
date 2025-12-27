using UnityEngine;

public interface ICombatEncounter
{
    string CombatSceneAddress { get; }
    string GetPoolKey();
}
