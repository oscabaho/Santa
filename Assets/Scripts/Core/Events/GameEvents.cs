using UnityEngine;
using Santa.Core.Save;

namespace Santa.Core.Events
{
    // =============================
    //  CHARACTER AND COMBAT EVENTS
    // =============================

    /// <summary>
    /// Event published when the player wins a combat.
    /// </summary>
    public class CombatVictoryEvent
{
    public GameObject Enemy { get; }
    public CombatVictoryEvent(GameObject enemy) { Enemy = enemy; }
}

/// <summary>
/// Event published when the player performs an action that consumes stamina
/// and which should reset its recovery delay.
/// </summary>
public class PlayerActionUsedStaminaEvent
{
    public GameObject Player { get; }
    public int StaminaCost { get; }

    public PlayerActionUsedStaminaEvent(GameObject player, int staminaCost)
    {
        Player = player;
        StaminaCost = staminaCost;
    }
}

/// <summary>
/// Event published when the player dies.
/// </summary>
public class PlayerDiedEvent
{
    public GameObject PlayerObject { get; }

    public PlayerDiedEvent(GameObject playerObject)
    {
        PlayerObject = playerObject;
    }
}

/// <summary>
/// Evento que representa la muerte de una entidad (personaje, enemigo, etc.).
/// </summary>
public class CharacterDeathEvent
{
    public GameObject Entity { get; }

    public CharacterDeathEvent(GameObject entity)
    {
        Entity = entity;
    }
}

/// <summary>
/// Event published when the player's GameObject is instantiated in a scene.
/// </summary>
public class PlayerSpawnedEvent
{
    public GameObject PlayerObject { get; }

    public PlayerSpawnedEvent(GameObject playerObject)
    {
        PlayerObject = playerObject;
    }
}

/// <summary>
/// Event published when a game save is successfully loaded.
/// Signals to systems that they should restore state from the loaded save data.
/// </summary>
public class GameLoadedEvent
{
    public SaveData SaveData { get; }

    public GameLoadedEvent(SaveData saveData)
    {
        SaveData = saveData;
    }
}

// =============================
//  EVENTOS DE INVENTARIO Y OBJETOS
// =============================

/// <summary>
/// Event published when an item is used (potion, weapon, etc.).
/// </summary>
public class ItemUsedEvent
{
    public string ItemId { get; }
    public GameObject User { get; }
    public ItemUsedEvent(string itemId, GameObject user) { ItemId = itemId; User = user; }
}

    

// =============================
//  INTERACTION EVENTS
// =============================

/// <summary>
/// Published when an interactable object enters the player's range.
/// </summary>
public class InteractableInRangeEvent
{
    public readonly IInteractable Interactable;
    public InteractableInRangeEvent(IInteractable interactable) => Interactable = interactable;
}

/// <summary>
/// Published when an interactable object that was in range leaves the range.
/// </summary>
    public class InteractableOutOfRangeEvent
    {
        public readonly IInteractable Interactable;
        public InteractableOutOfRangeEvent(IInteractable interactable) => Interactable = interactable;
    }
}
