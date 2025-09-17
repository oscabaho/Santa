using UnityEngine;

/// <summary>
/// Event published to request that a player's state be restored from persistent data.
/// </summary>
public class PlayerStateRestoreRequestEvent
{
    public GameObject PlayerObject { get; }
    public PlayerPersistentData Data { get; }
    public ItemDatabase Database { get; }

    public PlayerStateRestoreRequestEvent(GameObject playerObject, PlayerPersistentData data, ItemDatabase database)
    {
        PlayerObject = playerObject;
        Data = data;
        Database = database;
    }
}
