using UnityEngine;

/// <summary>
/// Data transfer object representing a planned action during turn-based combat.
/// Canonical definition lives in Core so all assemblies reference the same type.
/// </summary>
public struct PendingAction
{
    public Ability Ability;
    public GameObject Caster;
    public GameObject PrimaryTarget;
}
