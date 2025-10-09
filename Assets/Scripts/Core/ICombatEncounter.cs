using UnityEngine;
using System.Threading.Tasks;

public interface ICombatEncounter
{
    string CombatSceneAddress { get; }
    Task<GameObject> InstantiateCombatSceneFallbackAsync();
}
