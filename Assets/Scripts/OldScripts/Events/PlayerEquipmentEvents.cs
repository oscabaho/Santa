using UnityEngine;

public class PlayerWeaponEquippedEvent
{
    public GameObject Player { get; }
    public WeaponInstance Weapon { get; }

    public PlayerWeaponEquippedEvent(GameObject player, WeaponInstance weapon)
    {
        Player = player;
        Weapon = weapon;
    }
}

public class PlayerWeaponUnequippedEvent
{
    public GameObject Player { get; }
    public WeaponInstance Weapon { get; }

    public PlayerWeaponUnequippedEvent(GameObject player, WeaponInstance weapon)
    {
        Player = player;
        Weapon = weapon;
    }
}
