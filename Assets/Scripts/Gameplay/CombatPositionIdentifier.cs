using UnityEngine;

public enum CombatPosition
{
    Unknown = 0,
    Left = 1,
    Center = 2,
    Right = 3
}

/// <summary>
/// Identifies the position of a combatant in the arena (Left, Center, Right).
/// Used by the UI to map health bars to the correct enemy.
/// </summary>
public class CombatPositionIdentifier : MonoBehaviour
{
    [SerializeField] private CombatPosition position = CombatPosition.Unknown;

    public CombatPosition Position
    {
        get => position;
        set => position = value;
    }
}
